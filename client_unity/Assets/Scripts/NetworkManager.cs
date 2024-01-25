using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using NativeWebSocket;

public class NetworkManager : MonoBehaviour
{
    WebSocket websocket;
    public GameObject[] networkObjects;
    //dictionary to store object properties networkId -> dictionary of properties
    /** 
        networkObjectProperties = {
            "uuid": {
                gameObject: GameObject,
                properties: {
                    
                }
            }
        }
    **/
    //public Dictionary<string, Dictionary<string, object>> networkObjectProperties = new Dictionary<string, Dictionary<string, object>>();
    public Dictionary<string, Dictionary<string, object>> networkObjectProperties = new Dictionary<string, Dictionary<string, object>>();
    public string sessionId = "";
    public string userId = "";
    //array to store function names to be called after some time and the time
    // Start is called before the first frame update
    async void Start()
    {
        websocket = new WebSocket("ws://ggj.playumi.com");
        if (sessionId.Length == 0)
        {            
            sessionId = Guid.NewGuid().ToString();
        }
        if (userId.Length == 0)
        {
            string tmp = PlayerPrefs.GetString("userId");            
            if (tmp != "")
            {
                userId = tmp;
            }
            else
            {
                userId = Guid.NewGuid().ToString();
                //save userId to player prefs
                PlayerPrefs.SetString("userId", userId);
            }

        }

        websocket.OnOpen += () =>
        {
            Debug.Log("Connection open!");
            Invoke("Connect", 2);
        };

        websocket.OnError += (e) =>
        {
            Debug.Log("Error! " + e);
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("Connection closed!");
        };

        websocket.OnMessage += (bytes) =>
        {
            Debug.Log("OnMessage!");
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            Debug.Log("OnMessage! " + message);
            EvaluateMessage(message);
        };

        // waiting for messages
        await websocket.Connect();        
    }
    void EvaluateMessage(string msg){
        Debug.Log(msg);
        NetworkMessage data = JsonConvert.DeserializeObject<NetworkMessage>(msg);
        //Json.FromJson<Dictionary<string, object>>(msg);
        Debug.Log(data);
        switch(data.type){
            case "connected":
                Connected(data);
                break;
            case "rpc_set":
                RpcSetReceive(data.objectId, data.component, data.key, data.value);
                break;
        }
    }
    void Connected(NetworkMessage data){
        var sceneState = data.sceneState;
        /**
            {
                uuuid: {position {
                    x: 0,
                    y: 0,

                }, rotation: {x: 0, y: 0, z: 0, w: 0    }}
            }
        **/
        //loop through networkObjects
        foreach (GameObject obj in networkObjects)
        {
            //get the networkId
            string networkId = obj.GetComponent<NetworkObject>().networkId;
            //check if the networkId is in the sceneState
            if (sceneState.ContainsKey(networkId))
            {
                //get the object state
                Dictionary<string, object> objectState = (Dictionary<string, object>)sceneState[networkId];
                //get the position
                Dictionary<string, object> position = (Dictionary<string, object>)objectState["position"];
                //get the rotation
                Dictionary<string, object> rotation = (Dictionary<string, object>)objectState["rotation"];
                //set the position
                obj.transform.position = new Vector3((float)position["x"], (float)position["y"], (float)position["z"]);
                //set the rotation
                obj.transform.rotation = new Quaternion((float)rotation["x"], (float)rotation["y"], (float)rotation["z"], (float)rotation["w"]);
            }
        }

    }
    void Update()
    {
        #if !UNITY_WEBGL || UNITY_EDITOR
        websocket.DispatchMessageQueue();
        #endif
        if (websocket.State == WebSocketState.Open)
        {
            //check for keyboard wasd
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))
            {
                
            }
        }
        // //loop function calls
        // foreach (KeyValuePair<string, float> entry in functionCalls)
        // {
        //     if (entry.Value > 0)
        //     {
        //         functionCalls[entry.Key] -= Time.deltaTime;
        //     }
        //     else
        //     {
        //         //call the function
        //         Invoke(entry.Key, 0);
        //         //remove the function from the list
        //         functionCalls.Remove(entry.Key);
        //     }
        // }
    }
    async void sendText(string text)
    {
        if (websocket.State == WebSocketState.Open)
        {
            // Sending plain text
            await websocket.SendText(text);
        }
    }
    async void registerObjects(){
        //send one big array of objects via message
        var message = new Dictionary<string, object>();
        message["type"] = "registerObjects";
        message["objects"] = networkObjects;
        var json = JsonUtility.ToJson(message);
        await websocket.SendText(json);
    }
    public Dictionary<string, object> addNetworkObject(GameObject obj, string optionalNetworkId)
    {
        if(optionalNetworkId.Length == 0)
        {
            optionalNetworkId = Guid.NewGuid().ToString();
        }
        //if the object has the NetworkObject component, get its networkId
        NetworkObject networkObject = obj.GetComponent<NetworkObject>();
        if(networkObject != null)
        {   
            if(networkObject.networkId.Length == 0){
                networkObject.networkId = optionalNetworkId;
            }
            optionalNetworkId = networkObject.networkId;
        }else{
            //add the NetworkObject component
            networkObject = obj.AddComponent<NetworkObject>();
            networkObject.networkId = optionalNetworkId;
        }
        Debug.Log("saving network object with id " + optionalNetworkId);
        networkObjectProperties[optionalNetworkId] = new Dictionary<string, object>();
        networkObjectProperties[optionalNetworkId]["gameObject"] = obj;
        return networkObjectProperties[optionalNetworkId];
    }
    Dictionary<string, object> GetNetworkObject(string networkId){
        return networkObjectProperties[networkId];
    }
    async void Connect()
    {
        Debug.Log("connecting");
        var message = new Dictionary<string, object>();
        message["type"] = "connect";
        message["sessionId"] = sessionId;
        message["userId"] = userId;
        var json = JsonConvert.SerializeObject(message);
        Debug.Log(json);
        await websocket.SendText(json);
    }
    async void Rpc(string objectId, string method, string[] args)
    {
        var message = new Dictionary<string, object>();
        message["userId"] = userId;
        message["type"] = "rpc";
        message["objectId"] = objectId;
        message["method"] = method;
        message["args"] = args;
        var json = JsonUtility.ToJson(message);
        if (websocket.State == WebSocketState.Open)
        {
            // Sending plain text
            await websocket.SendText(json);
        }

    }
    public async void RpcSet(string objectId, string component, string propertyName, object value)
    {
        var message = new Dictionary<string, object>();
        message["userId"] = userId;
        message["type"] = "rpc_set";
        message["objectId"] = objectId;
        message["key"] = propertyName;
        //check if value is vector3
        if (value.GetType() == typeof(Vector3))
        {
            var vectorValue = (Vector3)value;
            var val = new Dictionary<string, float>();
            val["x"] = vectorValue.x;
            val["y"] = vectorValue.y;
            val["z"] = vectorValue.z;
            message["value"] = val;
        } else
        {
            message["value"] = value;
        }
        message["component"] = component;
        var json = JsonConvert.SerializeObject(message);
        //var json = JsonUtility.ToJson(message);
        Debug.Log(json);
        if (websocket.State == WebSocketState.Open)
        {
            // Sending plain text
            await websocket.SendText(json);
        }

    }
    void RpcSetReceive(string objectId, string component, string propertyName, Dictionary<string, float> value)
    {
        

        Debug.Log("RpcSetReceive>>>>>>>>>>>>");

        //get network object
        Dictionary<string, object> networkObject = GetNetworkObject(objectId);
        //get gameObject
        GameObject obj = (GameObject)networkObject["gameObject"];
        //check if the component exists
        if(component.Length > 0){
            if(component == "transform"){
                //check if the property exists
                if(propertyName == "position"){
                    //set the position
                    obj.transform.position = new Vector3((float)value["x"], (float)value["y"], (float)value["z"]);
                }
                if(propertyName == "rotation"){
                    //set the rotation
                    obj.transform.rotation = new Quaternion((float)value["x"], (float)value["y"], (float)value["z"], (float)value["w"]);
                }
            }else{
                //get the component
                Component comp = obj.GetComponent(component);
                //check if the component exists            
                if(comp){
                    //set the property
                    comp.GetType().GetProperty(propertyName).SetValue(comp, value);
                }
            }
        }else{
            //set the property
            obj.GetType().GetProperty(propertyName).SetValue(obj, value);
        }
    }
    void RpcReceive(string objectId, string component, string method, string[] args)
    {
        //get network object
        Dictionary<string, object> networkObject = GetNetworkObject(objectId);
        //get gameObject
        GameObject obj = (GameObject)networkObject["gameObject"];
        //check if the method exists
        if(component.Length > 0){
            //get the component
            Component comp = obj.GetComponent(component);
            //check if the method exists
            if(comp){
                //get the method
                System.Reflection.MethodInfo mi = comp.GetType().GetMethod(method);
                //check if the method exists
                if(mi != null){
                    //invoke the method
                    mi.Invoke(comp, args);
                }
            }
        }else{
            //get the method
            System.Reflection.MethodInfo mi = obj.GetType().GetMethod(method);
            //check if the method exists
            if(mi != null){
                //invoke the method
                mi.Invoke(obj, args);
            }
        }
    }
    private async void OnApplicationQuit()
    {
        await websocket.Close();
    }

}