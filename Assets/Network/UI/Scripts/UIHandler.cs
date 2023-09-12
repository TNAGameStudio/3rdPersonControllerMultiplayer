using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;


public class UIHandler : MonoBehaviour
{
    public void OnServer()
    {
        NetworkManager.Singleton.StartServer();
    }

    public void OnHost()
    {
        NetworkManager.Singleton.StartHost();
        //Cursor.visible = false;
        //Cursor.lockState = CursorLockMode.Locked;
    }

    public void OnClient()
    {
        NetworkManager.Singleton.StartClient();
    }
}
