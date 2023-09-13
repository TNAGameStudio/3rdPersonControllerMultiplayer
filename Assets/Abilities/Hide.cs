using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;


public class Hide : NetworkBehaviour
{
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
        HidePlayerClientRpc(isHiding, clientid);
    }

    [ClientRpc]
    private void HidePlayerClientRpc(bool isHiding, ulong clientid)
    {
        // doesn't need to hide or unhide own character
        if(IsOwner) return; //TODO: consider enabling a nice sneaky animation here.

        if(isHiding)
        {
            // Hide the originator of RPC in the world

        }
        else
        {
            // Unhide the originator of the RPC in the world
        }
    }


}
