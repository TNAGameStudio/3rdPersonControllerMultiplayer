using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;
using System;



public class Hide : NetworkBehaviour
{
     [SerializeField] Player ourPlayer;
     Renderer _renderer;
     private StarterAssetsInputs _input;
     int HIDING_LAYER = 6;
     int UNHIDING_LAYER = 0;
     bool isHiding = false;
     float HideCooldown = 3.0f; // 3 seconds
     bool onCooldown = false;

    public Material m_hide_body;
    public Material m_hide_arms;
    public Material m_hide_legs;
    public Material m_unhide_body;
    public Material m_unhide_arms;
    public Material m_unhide_legs;

     
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
            {
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, HIDING_LAYER);
            }
            else
            {
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, UNHIDING_LAYER);
            }

            // play animation of opposing player as they hide or unhide
            PlayHideUnhideAnimation(hidingPlayer, isHiding);

        }

          // tell all clients to hide this client
        HidePlayerClientRpc(isHiding, clientid);
    }

    private void PlayHideUnhideAnimation(Player hidingPlayer, bool isHiding)
    {
        

        if(isHiding)
        {


            StartCoroutine(FadeOut(hidingPlayer));

        }
        else
        {
            StartCoroutine(FadeIn(hidingPlayer));
            
        }
        
        

    }
    IEnumerator FadeOut(Player hidingPlayer)
    {
        Renderer _renderer = hidingPlayer.GetComponentInChildren<Renderer>();
        Material[] hideMaterials = new Material[3];

        hideMaterials[0] = m_hide_body;
        hideMaterials[1] = m_hide_arms;
        hideMaterials[2] = m_hide_legs;

        _renderer.materials = hideMaterials;


        for (float alpha = 0.0f; alpha < 1; alpha += 0.01f)
        {

            Debug.Log("Alpha:" + alpha);
            _renderer.materials[0].SetFloat("_Progress", alpha);
            _renderer.materials[1].SetFloat("_Progress", alpha);
            _renderer.materials[2].SetFloat("_Progress", alpha);
            yield return new WaitForSeconds(.01f);
        }
        
        // round off any inprecision
        _renderer.materials[0].SetFloat("_Progress", 1);
        _renderer.materials[1].SetFloat("_Progress", 1);
        _renderer.materials[2].SetFloat("_Progress", 1);

    }
    IEnumerator FadeIn(Player hidingPlayer)
    {
        Renderer _renderer = hidingPlayer.GetComponentInChildren<Renderer>();
        Material[] hideMaterials = new Material[3];

        for (float alpha = 1.0f; alpha > 0; alpha -= 0.01f)
        {
            Debug.Log("Fading in " + alpha);
            _renderer.materials[0].SetFloat("_Progress", alpha);
            _renderer.materials[1].SetFloat("_Progress", alpha);
            _renderer.materials[2].SetFloat("_Progress", alpha);
            yield return new WaitForSeconds(.01f);
        }

        // round off any inprecision
        _renderer.materials[0].SetFloat("_Progress", 0);
        _renderer.materials[1].SetFloat("_Progress", 0);
        _renderer.materials[2].SetFloat("_Progress", 0);

        hideMaterials[0] = m_unhide_body;
        hideMaterials[1] = m_unhide_arms;
        hideMaterials[2] = m_unhide_legs;

        _renderer.materials = hideMaterials;
    }

    private void SetHidingLayerMaskRecursively(GameObject playerGO, int masklayer)
    {
        //DEBUG
        return;

        playerGO.layer = masklayer;
        foreach (Transform child in playerGO.transform)
        {
            SetHidingLayerMaskRecursively(child.gameObject, masklayer);
        }
    }    

    [ClientRpc]
    private void HidePlayerClientRpc(bool isHiding, ulong clientid)
    {
        // Do not perform on ourselves.
        if(IsOwner)
            return;

    
            if(ourPlayer.playerList.TryGetValue(clientid, out Player hidingPlayer))
            {
                // TODO Handle any errors (though this is success case)
            } 

            if(isHiding)// hide them
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, HIDING_LAYER);
            else
                SetHidingLayerMaskRecursively(hidingPlayer.gameObject, UNHIDING_LAYER);

            // play animation of opposing player as they hide or unhide
            PlayHideUnhideAnimation(hidingPlayer, isHiding);                

    }


}
