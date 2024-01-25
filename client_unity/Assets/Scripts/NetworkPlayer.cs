using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkPlayer : NetworkObject
{
    public GameObject player;
    new void Start()
    {
        //get this gameobject
        player = this.gameObject;
        //call super class start
        base.Start();
    }

    // Update is called once per frame
    void Update()
    {
        //check for keyboard wasd while pressed        
        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {            
            //move the player z + 0.1 delta
            player.transform.position += new Vector3(0, 0, 0.1f);
            //networkManager.RpcSet();  
            //void RpcSet(string objectId, string component, string propertyName, object value)
            networkManager.RpcSet(networkId, "transform", "position", player.transform.position);
        }
        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            //move the player z - 0.1 delta
            player.transform.position += new Vector3(0, 0, -0.1f);
            //networkManager.RpcSet();  
            //void RpcSet(string objectId, string component, string propertyName, object value)
            networkManager.RpcSet(networkId, "transform", "position", player.transform.position);
        }
        if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.LeftArrow))
        {
            //move the player x - 0.1 delta
            player.transform.position += new Vector3(-0.1f, 0, 0);
            //networkManager.RpcSet();  
            //void RpcSet(string objectId, string component, string propertyName, object value)
            networkManager.RpcSet(networkId, "transform", "position", player.transform.position);
        }
        if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.RightArrow))
        {
            //move the player x + 0.1 delta
            player.transform.position += new Vector3(0.1f, 0, 0);
            //networkManager.RpcSet();  
            //void RpcSet(string objectId, string component, string propertyName, object value)
            networkManager.RpcSet(networkId, "transform", "position", player.transform.position);
        }
        
    }
}
