using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NetworkObject : MonoBehaviour
{
    // Start is called before the first frame update
    public string networkId;
    public NetworkManager networkManager;
    public void Start()
    {        
        //networkManager.addNetworkObject(this.gameObject, networkId);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void RpcSet(string component, string propertyName, object value){
        networkManager.RpcSet(networkId, component, propertyName, value);
    }
}
