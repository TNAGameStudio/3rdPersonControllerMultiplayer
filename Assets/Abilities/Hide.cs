using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;



public class Hide : NetworkBehaviour
{
     [SerializeField] Player ourPlayer;
     private StarterAssetsInputs _input;
     
    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()

    {
        if(_input.one == true)
        {
            ActivateAbility();

        }

    }

    public void ActivateAbility()
    {
        // TODO: Show icon somewhere that it's enabled hiding

        // send RPC to server telling other clients to hide this character
        Debug.Log("In ActivateAbility OwnerClientId: " + OwnerClientId);
        HidePlayerServerRpc(true, OwnerClientId);

    }

    public void DeactivateAbility()
    {
        // TODO: Remove icon shows it's enabled hiding

        // send RPC to server telling other clients to reveal this character
        HidePlayerServerRpc(false, OwnerClientId);
    }

    [ServerRpc]
    private void HidePlayerServerRpc(bool isHiding, ulong clientid)
    {
        Debug.Log("In HidePlayerServerRPC Client ID: " + clientid + " Owner id: " + OwnerClientId);

        // If it's anyone else but the host
        if(clientid != 0)
        {
            // TODO: Hide this player from me

            // get player from dictionary
            if(ourPlayer.playerList.TryGetValue(clientid, out Player hidingPlayer))
            {
                Debug.Log("Found the other player with client id: " + clientid + " going to hide them now.");
            }
        }

        // tell all clients to hide this client
        HidePlayerClientRpc(isHiding, clientid);

        Debug.Log("Hitting the server RPC");            

    }

    [ClientRpc]
    private void HidePlayerClientRpc(bool isHiding, ulong clientid)
    {
        Debug.Log("Hitting the client RPC. OwnerId: " + OwnerClientId + " client id: "+ clientid);
        
        // Do not perform on ourselves.
        if(OwnerClientId == clientid)
            return;

          
        if(isHiding)
        {
            // Hide the originator of RPC in the world
            //Debug.Log("Client ID who is goign into hiding " + clientid + " After lookup: " + ourPlayer.playerList[clientid].OwnerClientId);

        


        }
        else
        {
            // Unhide the originator of the RPC in the world
        }
    }


}
