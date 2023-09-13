using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;
using System;



public class Hide : NetworkBehaviour
{
     [SerializeField] Player ourPlayer;
     private StarterAssetsInputs _input;
     int HIDING_LAYER = 6;
     int UNHIDING_LAYER = 0;
     bool isHiding = false;
     float HideCooldown = 3.0f; // 3 seconds
     bool onCooldown = false;
     
    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
    }

    void Update()

    {
        if(onCooldown == true)
        {
            HideCooldown -= Time.deltaTime;

            if(HideCooldown <= 0)
            {
                
                onCooldown = false;
                HideCooldown = 3.0f;
            }
            else
            {
                return;
            }
        }

        if(_input.one == true)
        {
            //set cooldown
            onCooldown = true;
            

            if(isHiding == false)
            {
                ActivateAbility();
                isHiding = true;
            }
            else
            {
                DeactivateAbility(); 
                isHiding = false;
            }

            

        }

    }

    public void ActivateAbility()
    {
        // TODO: Show icon somewhere that it's enabled hiding

        // send RPC to server telling other clients to hide this character
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

            if(isHiding)// hide them
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, HIDING_LAYER);
            else
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, UNHIDING_LAYER);
        }

        // tell all clients to hide this client
        HidePlayerClientRpc(isHiding, clientid);
    }

    private void SetHidingLayerMaskRecursively(GameObject playerGO, int masklayer)
    {
        playerGO.layer = masklayer;
        foreach (Transform child in playerGO.transform)
        {
            SetHidingLayerMaskRecursively(child.gameObject, masklayer);
        }
    }    

    [ClientRpc]
    private void HidePlayerClientRpc(bool isHiding, ulong clientid)
    {
        Debug.Log("Hitting the client RPC. OwnerId: " + OwnerClientId + " client id: "+ clientid);
        
        // Do not perform on ourselves.
        if(OwnerClientId == clientid)
            return;

          
            if(ourPlayer.playerList.TryGetValue(clientid, out Player hidingPlayer))
            {
                // TODO Handle any errors (though this is success case)
            } 

            if(isHiding)// hide them
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, HIDING_LAYER);
            else
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, UNHIDING_LAYER);

    }


}
