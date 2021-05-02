using UnityEngine;
using System;

public class CoinMoveState : CoinState 
{

    public CoinMoveState(CoinStateID id, CoinController entity, int animationHash) : base(id, entity, animationHash){}

    public override Enum Tick() 
    {   
        var direction = (int) _entity.Facing;
        var speedX = _entity.Data.MoveSpeed * Time.deltaTime * direction; // TODO once the value is final, move to constructor or declare as field
        var speedY = 0f;

        if (!_entity.IsTouchingGround && !_entity.IsTouchingSpawn)
            speedY = _gravityVector.y * Time.deltaTime;

        _entity.MoveStep(speedX, speedY);

        if (_entity.IsTouchingEntityFront)
            _entity.ReverseFacing(); // TODO test if a dedicated state is necessary
        
        return null;
    }
}