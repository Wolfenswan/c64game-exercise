using UnityEngine;
using System;

public class EnemyFallState : EnemyState 
{
    public EnemyFallState(EnemyStateID id, EnemyController entity, int? animationHash = null) : base(id, entity, animationHash) 
    {
        CanBeFlipped = false;
        GivePointsOnFlip = false;
    }

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);
    }

    public override Enum Tick()
    {   
        var speedX = _entity.MoveSpeed * Time.deltaTime * (int) _entity.Facing;
        var speedY = _gravityVector.y * Time.deltaTime;
        _entity.MoveStep(speedX, speedY);
        
        if (_entity.IsTouchingGround)
            return EnemyStateID.MOVE;
        
        return null;
    }
}

// Fall is easy, as enemies always fall in an arc
// Just modify transform as needed