using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public class PlayerCombatStateMachine : MonoBehaviour
{
    public delegate void EnterStateEvent(string state);
    public delegate void ExitStateEvent(string state);

    public class PlayerCombatState
    {
        public string name;  //key

        public bool translationalMovementAllowed;
        public bool interruptableWithTranslationalMovement;
        public bool interruptableWithEscape;

        public EnterStateEvent OnEnterState;
        public ExitStateEvent OnExitState;
    }

    private Dictionary<string, PlayerCombatState> States;
    private PlayerCombatState CurrentState;
    private bool changingState = false;

    public void Awake()
    {
        States = new Dictionary<string, PlayerCombatState>();
        PlayerCombatState defaultState = new PlayerCombatState();
        defaultState.name = "default";
        defaultState.translationalMovementAllowed = true;
        defaultState.interruptableWithTranslationalMovement = false;
        defaultState.interruptableWithEscape = false;
        AddState(defaultState);

        CurrentState = defaultState;
    }
    public bool AddState(PlayerCombatState newState)
    {
        if(States.ContainsKey(newState.name))
        {
            Debug.Log("PlayerCombatStateMachiene::AddState() State of the same name already exists");
            return false;
        }

        States.Add(newState.name, newState);
        return true;
    }
    public bool ChangeState(string state)
    {   
        if(changingState)
        {
            Debug.LogError("Attempting to change state recursively. it is not supported at the moment for simplicity, try to move change state logic out of OnExitState/OnEnterState");
            return false;
        }

        if(CurrentState != null && CurrentState.name == state)
        {
            return true;
        }

        if(States.TryGetValue(state, out PlayerCombatState newState))
        {
            changingState = true;


            //we dont know what these do and dont want the state machine to end up in a bad state 
            try
            {
                //maybe check if state change is allowed, for now all state chagnes are allowed
                Debug.Log("calling current state exit: " + CurrentState.name);
                CurrentState.OnExitState?.Invoke(state);

                Debug.Log("calling next state enter: " + newState.name);
                newState.OnEnterState?.Invoke(CurrentState.name);
            }
            catch (System.Exception ex)
            {
                // This block catches all exceptions of type Exception or its derived types
                Debug.Log($"An exception occurred while changing states ({state})->({CurrentState.name}): {ex.Message}");
            }
            finally
            {
                CurrentState = newState;
                changingState = false;
            }
        }
        else
        {
            Debug.Log("PlayerCombatStateMachienePlayerCombatStateMachiene::ChangeState() requested to change to a non existant state(" + state + ")");
            return false;
        }

        return true;
    }

    public PlayerCombatState GetCurrentState() 
    {
        return CurrentState;
    }

    public bool IsInState(string stateName)
    {
        return CurrentState.name == stateName;
    }

    public bool ChangeToDefaultState()
    {
        return ChangeState("default");
    }

    void Start()
    {
  
    }

    // Update is called once per frame
    void Update()
    {
        //TODO: Hook this up via the input class so we can get it out of update
        if(Input.GetKeyDown(KeyCode.Escape)) 
        { 
            if(CurrentState.interruptableWithEscape)
            {
                ChangeToDefaultState();
            }
        }
    }
}
