using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Networking;
using Cinemachine;
using UnityEngine.InputSystem;
using Unity.VisualScripting;



public class Player : NetworkBehaviour 
{

    [SerializeField] private CinemachineVirtualCamera followCam;
    [SerializeField] private int ownerPriority = 15;
    [SerializeField] private PlayerInput playerInput;

    public Dictionary<ulong, Player> playerList = new Dictionary<ulong, Player>();

    override public void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            followCam.Priority = ownerPriority;
            playerInput.enabled = true;
        }

        // save all the players and their IDs in dictionary for quick reference lookup later
        playerList.Add(OwnerClientId, this);

    }
    
}



