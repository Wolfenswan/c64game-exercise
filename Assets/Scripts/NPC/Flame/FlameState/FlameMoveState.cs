using UnityEngine;
using System;

public class FlameMoveState : FlameState 
{

    public FlameMoveState(FlameStateID id, FlameController entity, int animationHash, bool doesHurt = true) : base(id, entity, animationHash, doesHurt){}
    bool _bouncing;

    // TODO maybe just use a bezierCurve?

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _bouncing = false;
    }

    public override Enum Tick() 
    {   
        var speed = _entity.Data.MoveSpeed * Time.deltaTime * (int) _entity.Facing; // TODO once the value is final, move to constructor or declare as field
        _entity.MoveStep(speed, 0f);

        if (_runTime >= _entity.Data.BounceFrequency)
            return FlameStateID.BOUNCE;

        return null;
    }
}