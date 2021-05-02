using UnityEngine;
using System;

public class FlameMoveState : FlameState 
{

    public FlameMoveState(FlameStateID id, FlameController entity, int animationHash, bool doesHurt = true) : base(id, entity, animationHash, doesHurt){}

    public override Enum Tick() 
    {   
        var direction = (int) _entity.Facing;
        var speedX = _entity.Data.MoveSpeed * Time.deltaTime * direction; // TODO once the value is final, move to constructor or declare as field
        var speedY = 0f;

        _entity.MoveStep(speedX, speedY);

        // TODO add: bounce
        
        return null;
    }
}