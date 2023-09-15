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
     bool isHiding = false;
     float HideCooldown = 3.0f; // 3 seconds
     bool onCooldown = false;

    Shader transparent;
    Shader originalShader;

            
     
    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        
            //transparent = Shader.Find("Shader Graphs/Vanish"); // vanish shader
            transparent = Shader.Find("Shader Graphs/Dissolve"); // dissolve
            if( transparent == null)
                Debug.Log("Could not find the vanish shader.");
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
        // If it's anyone else but the host
        if(clientid != 0)
        {
            // TODO: Hide this player from me

            // get player from dictionary
            if(ourPlayer.playerList.TryGetValue(clientid, out Player hidingPlayer))
            {
                Debug.Log("Found the other player with client id: " + clientid + " going to hide them now.");
            }

            // play animation of opposing player as they hide or unhide
            PlayHideUnhideAnimation(hidingPlayer, isHiding);

        }

          // tell all clients to hide this client
        HidePlayerClientRpc(isHiding, clientid);
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

        // play animation of opposing player as they hide or unhide
        PlayHideUnhideAnimation(hidingPlayer, isHiding);                

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

        //save original shader
        originalShader = _renderer.materials[0].shader;
        
        // assign new shader to all materials
        for(int i = 0; i < _renderer.materials.Length; i++)
        {
            _renderer.materials[i].shader = transparent;
        }

        // transition the transparency
        for (float alpha = 1.0f; alpha >= 0; alpha -= 0.01f)
        {
            _renderer.materials[0].SetFloat("_Progress", alpha);
            _renderer.materials[1].SetFloat("_Progress", alpha);
            _renderer.materials[2].SetFloat("_Progress", alpha);
            yield return new WaitForSeconds(.01f);
        }
        
        // round off any inprecision
        _renderer.materials[0].SetFloat("_Progress", 0);
        _renderer.materials[1].SetFloat("_Progress", 0);
        _renderer.materials[2].SetFloat("_Progress", 0);
    }

    IEnumerator FadeIn(Player hidingPlayer)
    {
        Renderer _renderer = hidingPlayer.GetComponentInChildren<Renderer>();

        // transition the transparency
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

        //restore original shader
        for(int i = 0; i < _renderer.materials.Length; i++)
        {
            _renderer.materials[i].shader = originalShader;
        }
    }





}
