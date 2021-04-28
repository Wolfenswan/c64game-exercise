using System;
using System.Collections.Generic;
using UnityEngine;

public enum TickMode
{
    UPDATE,
    FIXED_UPDATE,
    LATE_UPDATE,
    MANUAL
}

public class StateMachine 
{
    private State _currentState;
    private State _overrideOnNextTick;
    private Dictionary<Enum, State> _states = new Dictionary<Enum, State>();
    private bool _debug;
    
    public State CurrentState {get => _currentState;}

    public StateMachine(List<State> stateCollection, Enum startingState, bool debug = false) 
    {  
        foreach (var state in stateCollection)
        {   
            _states.Add(state.ID, state);
        }
        _currentState = _states[startingState];
        _debug = debug;

        _currentState.OnEnter(startingState);
    }

    public void Tick(TickMode tickMode)
    {   
        if (_overrideOnNextTick != null) // Might need testing, if there's a benefit to override happening on all ticks or only on Update()-Tick
        {
            ChangeState(_overrideOnNextTick);
            _overrideOnNextTick = null;
        }

        Enum newStateID;
        switch (tickMode)
        {   
            case TickMode.UPDATE:
                newStateID = _currentState.Tick();
                break;
            case TickMode.FIXED_UPDATE:
                newStateID = _currentState.FixedTick();
                break;
            case TickMode.LATE_UPDATE:
                newStateID = _currentState.LateTick();
                break;
            case TickMode.MANUAL:
                newStateID = _currentState.ManualTick();
                break;
            default:
                Debug.LogError($"StateMachine: Unknown tickMode {tickMode}.");
                newStateID = null;
                break;
        }

        if (newStateID != null) 
        {
            State newState = _states[newStateID];

            if (newState == _currentState)
                Debug.LogWarning($"StateMachine: Changing from state to instance of the same state. This could be desired. State: {newState}");

            if (newState != null) 
                ChangeState(newState);
            else if (newState == null) 
                Debug.LogError($"StateMachine: Tried to change from {_currentState} to {newStateID}, but it is not in List of states:{_states}.");
        }  
    }

    public void ForceState(Enum id) 
    {
        if (_states.ContainsKey(id)) 
            _overrideOnNextTick = _states[id];
        else 
            Debug.LogError($"StateMachine: Trying to enforce state with ID:{id}, but it does not exist in keys: {_states.Keys}");
    }

    void ChangeState(State newState)
    {   
        _currentState?.OnExit(newState.ID);

        if (_debug)
            Debug.Log($"StateMachine: Exited {_currentState.GetType().Name} @ {Time.time}. Entered {newState.GetType().Name}.");
        
        newState.OnEnter(_currentState.ID);
        _currentState = newState;
    }
}