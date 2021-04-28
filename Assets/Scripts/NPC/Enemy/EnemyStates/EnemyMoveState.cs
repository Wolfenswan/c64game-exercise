using UnityEngine;
using System;

public class EnemyMoveState : EnemyState 
{
    public EnemyMoveState(EnemyStateID id, EnemyController entity, int animationHash) : base(id, entity, animationHash) {}

    Vector3 _moveVector = new Vector3(0f,0f,0f);

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);
    }

    public override Enum Tick() 
    {   
        var direction = _entity.Facing;
        var speed = _entity.Data.MoveSpeed; // TODO once the value is final, move to constructor or declare as field

        _moveVector.x = (speed * Time.deltaTime) * direction;
        _entity.transform.position += _moveVector;

        if (!_entity.IsTouchingGround)
            return EnemyStateID.FALL;

        if (_doFlip)
            return EnemyStateID.FLIPPED;

        if (_entity.IsFacingOtherEntity)
            return EnemyStateID.TURN;
        
        return null;
    }
}