using System;
using System.Collections;
using System.Collections.Generic;
public class NetworkMessage{
    public string type;
    public string objectId;
    public string method;
    public string component;
    public string key;
    public Dictionary<string, float> value;
    public Dictionary<string, object> sceneState;
}