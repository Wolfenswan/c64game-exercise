using System;
using UnityEngine;

public class EnemyTurnState : EnemyState 
{
    public EnemyTurnState(EnemyStateID id, EnemyController entity, int animationHash) : base(id, entity, animationHash) {}

    public override void OnEnter(Enum toState)
    {
        base.OnEnter(toState);

        _entity.Facing *= -1;
        _entity.transform.localScale *= -1;
    }

    public override Enum Tick()
    {
        return EnemyStateID.MOVE;
    }

    // TODO subscribe to AnimationFinishedEvent
    // -> when done: change to MoveState
}