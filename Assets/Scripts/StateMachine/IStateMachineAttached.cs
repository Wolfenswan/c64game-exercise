using System;

interface IStateMachineAttached
{
    StateMachine StateMachine{get;}
    State CurrentState{get;}
    Enum CurrentStateID{get;}
    string CurrentStateName{get;}

    void InitializeStateMachine();
}