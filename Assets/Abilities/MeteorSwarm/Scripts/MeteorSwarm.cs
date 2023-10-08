using StarterAssets;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using Unity.Netcode;
using Unity.VisualScripting;
using UnityEngine;

public class MeteorSwarm : NetworkBehaviour
{
    [Header("References")]
    [SerializeField] public GameObject clientAOEPrefab;
    [SerializeField] public GameObject clientCharacterEffectPrefab;
    [SerializeField] public GameObject clientAOEPlacement;

    [Header("Parameters")]
    [SerializeField] float damagePerTick = 10.0f;
    [SerializeField] float timePerTick = 0.5f;
    [SerializeField] int numTicks = 6;
    [SerializeField] float cooldown = 30.0f;

    ManagedCooldown cooldownManager;
    AOETargeting meteorSwarmTargeting;
    private PlayerCombatStateMachine CombatStateMachine;

    private StarterAssetsInputs _input;

    private Animator _animator; // for setting the sneak/hide animation
    private bool _hasAnimator;

    private Transform leftHandTransform;
    private Transform rightHandTransform;

    bool abilityActive = false;

    const string meteorSwarmPlacement = "meteor_swarm_placement";
    const string meteorSwarmSpell = "meteor_swarm";
    int cooldownId = 0;

    // Start is called before the first frame update
    void Start()
    {
        CombatStateMachine = GetComponent<PlayerCombatStateMachine>();
        PlayerCombatStateMachine.PlayerCombatState aoePlacement = new PlayerCombatStateMachine.PlayerCombatState();
        aoePlacement.name = meteorSwarmPlacement;
        aoePlacement.translationalMovementAllowed = true;
        aoePlacement.interruptableWithTranslationalMovement = false;
        aoePlacement.interruptableWithEscape = true;
        aoePlacement.OnEnterState = OnEnterTargetingState;
        aoePlacement.OnExitState = OnExitTargetingState;

        CombatStateMachine.AddState(aoePlacement);

        PlayerCombatStateMachine.PlayerCombatState nonInterruptableCast = new PlayerCombatStateMachine.PlayerCombatState();
        nonInterruptableCast.name = meteorSwarmSpell;
        nonInterruptableCast.translationalMovementAllowed = false;
        nonInterruptableCast.interruptableWithTranslationalMovement = false;
        nonInterruptableCast.interruptableWithEscape = true;
        nonInterruptableCast.OnEnterState = OnEnterCastState;
        nonInterruptableCast.OnExitState = OnExitCastState;
        CombatStateMachine.AddState(nonInterruptableCast);

        _input = GetComponent<StarterAssetsInputs>();

        // Get the animator
        _hasAnimator = TryGetComponent(out _animator);

        // check for errors
        if (_hasAnimator == false)
            Debug.Log("(MeteorSwarm.cs) Could not find animator.");

        leftHandTransform = HelperFunctions.FindObjectWithTag(transform, "LeftHand").transform;
        rightHandTransform = HelperFunctions.FindObjectWithTag(transform, "RightHand").transform;

        if (!leftHandTransform || !rightHandTransform)
        {
            Debug.Log("MeteorSwarm.cs could not find left or right hand transform.  need to tag them in the model");
        }

        meteorSwarmTargeting = GetComponent<AOETargeting>();
        cooldownId = cooldownManager.GetCooldownId(meteorSwarmSpell);
    }

    public override void OnNetworkSpawn()
    {
        cooldownManager = GetComponent<ManagedCooldown>();
        cooldownManager.SetCooldown(meteorSwarmSpell, cooldown);
    }

    public override void OnNetworkDespawn()
    {

    }
    void OnEnterCastState(string state)
    {
        if (cooldownManager.OnCooldown(cooldownId))
        {
            //this should never happen.
            Debug.LogError("meteor swarm casting state entered while spell on cooldown");
            return;
        }
        _animator.SetBool("CastingMeteorSwarm", true);
    }

    void OnExitCastState(string state)
    {
        _animator.SetBool("CastingMeteorSwarm", false);
        abilityActive = false;
    }

    //these
    void OnEnterTargetingState(string state)
    {
        meteorSwarmTargeting.Activate(clientAOEPlacement);
    }

    void OnExitTargetingState(string state)
    {
        meteorSwarmTargeting.Deactivate();

        if (state != meteorSwarmSpell)
        {
            abilityActive = false;
        }
    }

    void ActivateAbility()
    {
        if (abilityActive)
        {
            return;
        }

        abilityActive = true;
        CombatStateMachine.ChangeState(meteorSwarmPlacement);
    }
    void DeactivateAbility()
    {
        if (!abilityActive)
        {
            return;
        }

        abilityActive = false;
        CombatStateMachine.ChangeToDefaultState();
    }

    [ServerRpc]
    void CastMeteorSwarmServerRpc(Vector3 position, Vector3 orientation)
    {
        //check cooldown
        if (cooldownManager.OnCooldown(cooldownId))
        {
            Debug.LogError("Server is on cooldown");
            return;
        }
        cooldownManager.ConsumeCooldown(cooldownId);
        CastMeteorSwarmClientRpc(position, orientation);
        //TODO: Add damage
        //SpawnServerDamageOrb();
    }

    [ClientRpc]
    void CastMeteorSwarmClientRpc(Vector3 position, Vector3 orientation)
    {
        if(IsOwner)
        {
            return;
        }

        //this game object should self destroy after it's finished
        GameObject AOEInstance = Instantiate(clientAOEPrefab, position, Quaternion.identity);
        AOEInstance.transform.up = orientation;
    }

    // Update is called once per frame
    void Update()
    {
        if (!IsOwner)
        {
            return;
        }

        if(cooldownManager.OnCooldown(cooldownId))
        {
            return;
        }

        
        // If user has selected to go into or out of hide mode
        // TODO: Remove old input system
        if(abilityActive)
        {
            //Pool for left click
            if (Input.GetMouseButtonDown(0))
            {
                CombatStateMachine.ChangeState(meteorSwarmSpell);
            }

        }
        else
        {
            //make active if need be
            if ( _input.aoe)
            {
                ActivateAbility();
            }
        }

    }

    //animation event triggers this
    public void CastSpell()
    {
        if(!IsOwner)
        {
            return;
        }

        if (!CombatStateMachine.IsInState(meteorSwarmSpell))
        {
            return;
        }

        CastMeteorSwarmServerRpc(meteorSwarmTargeting.LastHit.point, meteorSwarmTargeting.LastHit.normal);
        //this game object should self destroy after it's finished
        GameObject AOEInstance = Instantiate(clientAOEPrefab, meteorSwarmTargeting.LastHit.point, Quaternion.identity);
        AOEInstance.transform.up = meteorSwarmTargeting.LastHit.normal;
        DeactivateAbility();
    }

    public void OnCastAnimationStart()
    {
        //assumes these have the lifespan script attached to them for cleanup
        GameObject _temp = Instantiate(clientCharacterEffectPrefab, leftHandTransform);
        Instantiate(clientCharacterEffectPrefab, rightHandTransform);

        //TODO: check to make sure they have a lifespan script and issue error otherwise
        if(!GetComponent<Lifetime>())
        {
            Debug.LogError("MeteorSwarm.cs lifetime");
        }
    }
    public void OnCastAnimationEnd()
    {
        //doesn't work here
        _animator.SetBool("CastingMeteorSwarm", false);

    }
}