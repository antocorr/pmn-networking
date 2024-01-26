using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ConnectButton : MonoBehaviour
{
    public NetworkManager networkManager;
    public GameObject textInputField;
    public GameObject userNameInputField;
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
    public void OnClick(){
        string sessionId = textInputField.GetComponent<TMP_Text>().text;
        networkManager.sessionId = sessionId;
        networkManager.userName = userNameInputField.GetComponent<TMP_Text>().text;
        networkManager.StartConnection();
    }

}
