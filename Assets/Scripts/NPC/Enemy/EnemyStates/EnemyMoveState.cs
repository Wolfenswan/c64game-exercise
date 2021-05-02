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
        var direction = (int) _entity.Facing;
        var speed = _entity.Data.MoveSpeed; // TODO once the value is final, move to constructor or declare as field

        _entity.MoveStep((speed * Time.deltaTime) * direction, 0f);

        if (!_entity.IsTouchingGround && !_entity.IsTouchingSpawn)
            return EnemyStateID.FALL;

        if (_doFlip)
            return EnemyStateID.FLIPPED;

        if (_entity.IsTouchingEntityFront)
            return EnemyStateID.TURN;
        
        return null;
    }
}