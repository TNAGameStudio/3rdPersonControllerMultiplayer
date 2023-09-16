using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;
using System;
using Unity.VisualScripting;



public class Hide : NetworkBehaviour
{
     [SerializeField] Player ourPlayer;
     Renderer _renderer;
     private StarterAssetsInputs _input;
     bool isHiding = false;
     float HideCooldown = 3.0f; // 3 seconds
     bool onCooldown = false;

    Shader hideShader;
    Shader originalShader;

    private Animator _animator; // for setting the sneak/hide animation
    private bool _hasAnimator;

    [SerializeField] public Material materialNotHiding;
    [SerializeField] private Material materialIsHiding;

            
     
    // Start is called before the first frame update
    void Start()
    {
        _input = GetComponent<StarterAssetsInputs>();
        
        //hideShader = Shader.Find("Shader Graphs/Vanish"); // vanish shader
        hideShader = Shader.Find("Shader Graphs/Dissolve"); // dissolve
        
        // check for errors
        if( hideShader == null)
            Debug.Log("Could not find the vanish shader.");

        // Get the animator
        _hasAnimator = TryGetComponent(out _animator);

        // check for errors
        if(_hasAnimator == false)
            Debug.Log("(Hide.cs) Could not find animator.");
    }

    void Update()

    {
        // Calculate and reset cooldowns
        if(onCooldown == true)
        {
            // decrease cooldown time
            HideCooldown -= Time.deltaTime;

            // if cooldown has expired
            if(HideCooldown <= 0)
            {
                // reset cooldown
                onCooldown = false;
                HideCooldown = 3.0f;
            }
            else
            {
                // return if it has not expired yet
                return;
            }
        }

        // If user has selected to go into or out of hide mode
        if(_input.hide == true)
        {
            //set cooldown
            onCooldown = true;
            
            // if hiding, unhide and vice versa
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

        // Enable local effect on player character to show he's in sneak mode
        HidePlayerLocally(true);

        // Set the animation to sneak/hide mode
        _animator.SetBool("Hiding", true);

        // send RPC to server telling other clients to hide this character
        HidePlayerServerRpc(true, OwnerClientId);
    }

    public void DeactivateAbility()
    {
        // TODO: Remove icon shows it's enabled hiding

        // Enable local effect on player character to show she's no longer sneaking
        HidePlayerLocally(false);

        // Remove sneak/hide animation
        _animator.SetBool("Hiding", false);

        // send RPC to server telling other clients to reveal this character
        HidePlayerServerRpc(false, OwnerClientId);
    }

    private void HidePlayerLocally(bool isHiding)
    {
        // grab a reference to the renderer
        _renderer = GetComponentInChildren<Renderer>();

        // error check
        if(_renderer == null)
        {
            Debug.Log("Hide.cs/HidePlayerLocally(): Could not retrieve renderer");
            return;
        }

        // set the appropriate material depending on hiding or not
        if(isHiding)
        {
            _renderer.material = materialIsHiding;
        }
        else
        {
            _renderer.material = materialNotHiding;
        }

        return;
    }

    [ServerRpc]
    private void HidePlayerServerRpc(bool isHiding, ulong clientid)
    {
        // If it's anyone else but the host then hide this player from the server user
        if(clientid != 0)
        {
            // get player from dictionary
            if(!ourPlayer.playerList.TryGetValue(clientid, out Player hidingPlayer))
            {
                // Log error and return without doing anything.
                Debug.Log("Error finding the player we're searching for...");
                return;
            }

            // play effect of opposing player disappearing as they hide or unhide
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

    
        if(!ourPlayer.playerList.TryGetValue(clientid, out Player hidingPlayer))
        {
            // Handle any errors (though this is success case)
            Debug.Log("(HidePlayerClientRPC): Could not find player with player id:" + clientid);
            return;
        } 

        // play animation of opposing player as they hide or unhide
        PlayHideUnhideAnimation(hidingPlayer, isHiding);                

    }

    private void PlayHideUnhideAnimation(Player hidingPlayer, bool isHiding)
    {
    
        if(isHiding)
            StartCoroutine(FadeOut(hidingPlayer));
        else
            StartCoroutine(FadeIn(hidingPlayer));
    }

    IEnumerator FadeOut(Player hidingPlayer)
    {
        Renderer _renderer = hidingPlayer.GetComponentInChildren<Renderer>();

        //save original shader
        originalShader = _renderer.materials[0].shader;
        
        // assign new shader to all materials
        for(int i = 0; i < _renderer.materials.Length; i++)
        {
            _renderer.materials[i].shader = hideShader;
        }

        // transition the transparency
        for (float alpha = 1.0f; alpha >= 0; alpha -= 0.01f)
        {
            for(int i = 0; i < _renderer.materials.Length; i++)
            {
                _renderer.materials[i].SetFloat("_Progress", alpha);
            }
            yield return new WaitForSeconds(.01f);
        }
        
        // round off any inprecision
         for(int i = 0; i < _renderer.materials.Length; i++)
         {
            _renderer.materials[i].SetFloat("_Progress", 0);
        }
    }

    IEnumerator FadeIn(Player hidingPlayer)
    {
        Renderer _renderer = hidingPlayer.GetComponentInChildren<Renderer>();

        // transition the transparency
        for (float alpha = 0.0f; alpha < 1; alpha += 0.01f)
        {

             for(int i = 0; i < _renderer.materials.Length; i++)
             {
                _renderer.materials[i].SetFloat("_Progress", alpha);
             }
            
            yield return new WaitForSeconds(.01f);
        }
        
        // round off any inprecision
        
        for(int i = 0; i < _renderer.materials.Length; i++) 
        {
            _renderer.materials[i].SetFloat("_Progress", 1);
        }
        
        //restore original shader
        for(int i = 0; i < _renderer.materials.Length; i++)
        {
            _renderer.materials[i].shader = originalShader;
        }
    }
}
