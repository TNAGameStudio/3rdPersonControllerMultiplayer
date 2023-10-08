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
    private PlayerCombatStateMachine CombatStateMachine;

    const string fireballSpell = "fireball";
    private int cooldownId = 0;


    //this is called before start on network spawned objects
    public override void OnNetworkSpawn()
    {
        cooldownManager = GetComponent<ManagedCooldown>();
        cooldownManager.SetCooldown(fireballSpell, cooldown);
    }

    // Start is called before the first frame update
    void Start()
    {
        CombatStateMachine = GetComponent<PlayerCombatStateMachine>();
        PlayerCombatStateMachine.PlayerCombatState interruptableCast = new PlayerCombatStateMachine.PlayerCombatState();
        interruptableCast.name = fireballSpell;
        interruptableCast.translationalMovementAllowed = true;
        interruptableCast.interruptableWithTranslationalMovement = true;
        interruptableCast.interruptableWithEscape = true;
        interruptableCast.OnEnterState = OnEnterCastState;
        interruptableCast.OnExitState = OnExitCastState;
        CombatStateMachine.AddState(interruptableCast);
      
        _input = GetComponent<StarterAssetsInputs>();
        
        // Get the animator
        _hasAnimator = TryGetComponent(out _animator);

        // check for errors
        if(_hasAnimator == false)
            Debug.Log("(Fireball.cs) Could not find animator.");

        leftHandTransform = HelperFunctions.FindObjectWithTag(transform, "LeftHand").transform;
        rightHandTransform = HelperFunctions.FindObjectWithTag(transform, "RightHand").transform;
        CombatStateMachine = GetComponent<PlayerCombatStateMachine>();
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

        if (cooldownManager.OnCooldown(cooldownId))
        {
            return;
        }

        // If user has selected to go into or out of hide mode
        if (_input.fireball == true)
        {
            //change to fireball casting state, the on state enter has the animation logic
            CombatStateMachine.ChangeState(fireballSpell);            
        }
    }

    //This is triggered to an animation frame in Fireball.fbx animation
    public void Shoot()
    {
        if(!IsOwner)
        {
            return;
        }

        //if we the combat state is canceled(due to movement or escape) very close to
        //the animation fire event, the event can trigger while we are technically
        //not casting anymore, the bug cause two fireballs to spawn so handle it here.

        //TODO: we should probably be careful to add something similiar to all spells
        //that use animation events to trigger.
        if(!CombatStateMachine.IsInState(fireballSpell))
        {
            return;
        }

        //set fireball launch position and direction until we have aiming it's just this
        Vector3 fireballDirection = ourPlayer.transform.forward;
        Vector3 fireballPosition = leftHandTransform.position;

        //tell server we launched
        ShootFireballServerRpc(fireballPosition, fireballDirection);

        //launch our fireball
        ShootDummyFireball(fireballPosition, fireballDirection);
        CombatStateMachine.ChangeToDefaultState();

    }

    private void SpawnDummyFireball()
    {
        Debug.Log("Spawn Dummy");
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
        Debug.Log("Shoot Dummy Fireball");
        if (!clientFireball)
        {
            Debug.Log("ShootClientFireball(): Client Fireball not valid, probably lag");
            SpawnDummyFireball();
        }

        ShootFireball(clientFireball, Position, Direction);
        clientFireball = null;
    }

    private void ShootServerFireball(Vector3 Position, Vector3 Direction)
    {
        if(!serverFireball)
        {
            Debug.Log("ShootServerFireball(): Server Fireball not valid, should never happen");
            return;
        }

        ShootFireball(serverFireball, Position, Direction);
        serverFireball = null; 
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
    private void ShootFireballServerRpc(Vector3 Position, Vector3 Direction)
    {
        //check cooldown
        if (cooldownManager.OnCooldown(cooldownId))
        {
            Debug.LogError("Server: Fireball is on cooldown");
            return;
        }

        cooldownManager.ConsumeCooldown(cooldownId);

        //shoot server damaging fireball
        SpawnServerFireball();
        ShootServerFireball(Position, Direction);

        //tell clients we have fired
        ShootDummyFireballClientRpc(Position, Direction);
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
    
    public void OnCastAnimationStateEnter()
    {
        Debug.Log($"Animation state Fireball Start {NetworkObjectId}");

        if(clientFireball)
        {
            //for some reason if it's coming from network controld animator events it starts and
            //stops then starts again.
            return;
        }

        SpawnDummyFireball();
    }


    public void OnCastAnimationStateExit()
    {
        Debug.Log($"Animation state Fireball Exit {NetworkObjectId}");
    }

    //combat state machine
    void OnEnterCastState(string state)
    {
        Debug.Log("Combat state: Fireball Enter Cast");
        //start playing the fireball animation
        _animator.SetBool("CastingFireball", true);
    }

    void OnExitCastState(string state)
    {
        //can use exit time here to give the animation a little more time
        Debug.Log("Combat state: Fireball Exit Cast");
        _animator.SetBool("CastingFireball", false);


        if (clientFireball)
        {
            //we are still managing the fireball so delete it.
            Destroy(clientFireball);
        }

        serverFireball = null;
        clientFireball = null;
    }
}
