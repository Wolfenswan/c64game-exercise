using UnityEngine;
using System;

public class EnemyMoveState : EnemyState 
{

    public EnemyMoveState(EnemyStateID id, EnemyController entity, int animationHash) : base(id, entity, animationHash) 
    {
        CanBeFlipped = true;
        GivePointsOnFlip = true;
    }

    public override Enum Tick() 
    {   
        var speed = _entity.MoveSpeed * Time.deltaTime * (int) _entity.Facing; // TODO once the value is final, move to constructor or declare as field
        _entity.MoveStep(speed, 0f);

        if (!_entity.IsTouchingGround && !_entity.IsTouchingSpawnOrExit)
            return EnemyStateID.FALL;

        if (_doFlip)
            return EnemyStateID.FLIPPED;

        if (_entity.IsTouchingEntityFront)
            return EnemyStateID.TURN;
        
        return null;
    }
}