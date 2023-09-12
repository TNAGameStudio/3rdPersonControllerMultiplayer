using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Cinemachine;
using UnityEngine.InputSystem;



public class Player : NetworkBehaviour 
{

    [SerializeField] private CinemachineVirtualCamera followCam;
    [SerializeField] private int ownerPriority = 15;
    [SerializeField] private PlayerInput playerInput;

    override public void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            followCam.Priority = ownerPriority;
            playerInput.enabled = true;
        }
    }
    
}



