using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using StarterAssets;
using System;
using Unity.VisualScripting;
using Unity.Netcode.Components;
using UnityEngine.UIElements;
using TMPro;


public class Fireball : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] public Player ourPlayer;
    [SerializeField] public GameObject clientFireballPrefab;
    [SerializeField] public GameObject serverFireballPrefab;

    [Header("Parameters")]
    [SerializeField] public float initalVelocity = 25.0f;
    [SerializeField] float cooldown = 3.0f; // 3 seconds


    private StarterAssetsInputs _input;
   

    private Animator _animator; // for setting the sneak/hide animation
    private bool _hasAnimator;

    private GameObject clientFireball;
    private GameObject serverFireball;

    private Transform leftHandTransform;
    private Transform rightHandTransform;

    private ManagedCooldown cooldownManager;
    private PlayerCombatStateMachiene CombatStateMachiene;

    /*
    // Start is called before the first frame update
    void OnEnterCastState(string state)
    {

    }

    void OnExitCastState(string state)
    {

    }

    //attaching to animation state allows the clients to start vfx
    void OnCastAnimationStateEnter()
    {
        SpawnDummyFireball();
    }

    void OnCastAnimationStateExit()
    {

    }

    void Start()
    {
        PlayerCombatStateMachiene.PlayerCombatState interruptableCast = new PlayerCombatStateMachiene.PlayerCombatState();
        interruptableCast.name = "fireball_cast";
        interruptableCast.transationalMovementAllowed = true;
        interruptableCast.rotationalMovementAllowed = true;
        interruptableCast.interruptableWithMovement = true;
        interruptableCast.interruptableWithEscape = true;
        interruptableCast.OnEnterState = OnEnterCastState;
        interruptableCast.OnExitState = OnExitCastState;
        CombatStateMachiene.AddState(interruptableCast);
      
        _input = GetComponent<StarterAssetsInputs>();
        
        // Get the animator
        _hasAnimator = TryGetComponent(out _animator);

        // check for errors
        if(_hasAnimator == false)
            Debug.Log("(Fireball.cs) Could not find animator.");

        leftHandTransform = HelperFunctions.FindObjectWithTag(transform, "LeftHand").transform;
        rightHandTransform = HelperFunctions.FindObjectWithTag(transform, "RightHand").transform;
        CombatStateMachiene = GetComponent<PlayerCombatStateMachiene>();
        if (!leftHandTransform || !rightHandTransform)
        {
            Debug.Log("Fireball.cs could not find left or right hand transform.  need to tag them in the model");
        }

    }

    void Update()
    {
        if(!IsOwner) 
        { 
            return;  
        }

        // Calculate and reset cooldowns
        if(onCooldown == true)
        {
            // decrease cooldown time
            cooldown -= Time.deltaTime;

            // if cooldown has expired
            if(cooldown <= 0)
            {
                // reset cooldown
                onCooldown = false;
                cooldown = 3.0f;
            }
            else
            {
                // return if it has not expired yet
                return;
            }
        }

        // If user has selected to go into or out of hide mode
        if(_input.fireball == true)
        {
            ClientStartCast();

            //set cooldown
            onCooldown = true;
        }
    }

    public void ClientStartCast()
    {
        Debug.Log("Cleint Start Cast");
        if(!CombatStateMachiene.ChangeState("cast_fireball"))
        {
            Debug.Log("Unable to change state");
            return;
        }

        //tell server we started casting
        SpawnFireballServerRpc();

        //spawn our dummy fireball on this client
        SpawnDummyFireball();
    }

    //This is triggered to an animation frame in Fireball.fbx animation
    public void Shoot()
    {
        if(!IsOwner)
        {
            return;
        }


        Debug.Log("Shoot FireBall Animation Trigger");

        //tell animator we are done casting, it could technically be another event as it fires before the animation is finished
        _animator.SetBool("CastingFireball", false);

        //set fireball launch position and direction until we have aiming it's just this
        Vector3 fireballDirection = ourPlayer.transform.forward;
        Vector3 fireballPosition = leftHandTransform.position;

        //tell server we launched
        ShootFireballServerRpc(fireballPosition, fireballDirection);

        //launch our fireball
        ShootDummyFireball(fireballPosition, fireballDirection);
     }

    private void SpawnDummyFireball()
    {
        //start playing the fireball animation
        _animator.SetBool("CastingFireball", true);

        //attach to left hand
        clientFireball = Instantiate(clientFireballPrefab, leftHandTransform);

        //turn off physics so it doens't collide with player or have gravity applied
        Rigidbody fireballRB = clientFireball.GetComponent<Rigidbody>();
        fireballRB.isKinematic = true;

        //ignore collision with player, need to check if this is still necessary
        Physics.IgnoreCollision(ourPlayer.GetComponent<CharacterController>().GetComponent<Collider>(), clientFireball.GetComponent<Collider>());
    }

    private void ShootDummyFireball(Vector3 Position, Vector3 Direction)
    {
        if (!clientFireball)
        {
            Debug.Log("ShootClientFireball(): Server Fireball not valid");
            return;
        }

        ShootFireball(clientFireball, Position, Direction);
    }

    private void ShootServerFireball(Vector3 Position, Vector3 Direction)
    {
        if(!serverFireball)
        {
            Debug.Log("ShootServerFireball(): Server Fireball not valid");
        }

        ShootFireball(serverFireball, Position, Direction);
    }

    private void ShootFireball(GameObject fireball, Vector3 Position, Vector3 Direction)
    {
        Debug.Log("ShootFireball()");

        //reposition the fireball to the new position and align with direction
        Rigidbody fireballRB = fireball.GetComponent<Rigidbody>();

        //unhook the fireball from the hands if necessary(server fireballs never connected to hand)
        fireball.transform.parent = null;
        fireball.transform.position = Position;

        //for now until we get aiming or some other targeting system
        Vector3 forward = ourPlayer.transform.forward;

        //orient fireball
        fireball.transform.forward = Direction;

        //apply impulse
        fireballRB.isKinematic = false;
        fireballRB.velocity = Direction * initalVelocity;
    }

    void SpawnServerFireball()
    {
        serverFireball = Instantiate(serverFireballPrefab);

        //need to test if this is needed anymore
        Physics.IgnoreCollision(ourPlayer.GetComponent<CharacterController>().GetComponent<Collider>(), serverFireball.GetComponent<Collider>());
    }

    [ServerRpc]
    private void SpawnFireballServerRpc()
    {
        //tell clients we spawned
        SpawnDummyFireballClientRpc();
    }

    [ServerRpc]
    private void ShootFireballServerRpc(Vector3 Position, Vector3 Direction)
    {
        //shoot server damaging fireball
        SpawnServerFireball();
        ShootServerFireball(Position, Direction);

        //tell clients we have fired
        ShootDummyFireballClientRpc(Position, Direction);
    }


    [ClientRpc]
    private void SpawnDummyFireballClientRpc()
    {
        if(IsOwner)
        {
            return;
        }

        SpawnDummyFireball();
    }

    [ClientRpc]
    private void ShootDummyFireballClientRpc(Vector3 Position, Vector3 Direction)
    {
        if (IsOwner)
        {
            return;
        }

        //launch fireball
        ShootDummyFireball(Position, Direction);
    }
    */
}
