
using System;
using UnityEngine;

public class CoinCollectState : CoinState 
{

    public CoinCollectState(CoinStateID id, CoinController entity, int animationHash) : base(id, entity, animationHash){}

    float yForce;

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        yForce = 2; // TODO data and tweak as required
    }
    public override Enum Tick()
    {   
        var newY = yForce * Time.deltaTime;
        _entity.MoveStep(0f, newY);
        yForce += _gravityVector.y * Time.deltaTime;

        return null;
    }
}