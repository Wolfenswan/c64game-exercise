using UnityEngine;
using System;

public class FlameBounceState : FlameState 
{

    public FlameBounceState(FlameStateID id, FlameController entity, int animationHash, bool doesHurt = true) : base(id, entity, animationHash, doesHurt){}

    float _initialY;
    Vector2 _velocityVector;
    Vector2 _gravityVector;

    // TODO maybe just use a bezierCurve?

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _initialY = _entity.Pos.y;
        _gravityVector = GameManager.Instance.Data.GravityVector; // TODO move to constructor once final
        _velocityVector = new Vector2(_entity.Data.MoveSpeed * (int) _entity.Facing, _entity.Data.BounceStrength);  // TODO move to constructor once final
    }

    public override Enum Tick() 
    {   
        var bounceVector = _velocityVector * Time.deltaTime;
        _entity.MoveStep(bounceVector);
        _velocityVector += _gravityVector * Time.deltaTime;

        if (_runTime >= 0.05f && _entity.Pos.y <= _initialY)
            return FlameStateID.MOVE;

        return null;
    }
}