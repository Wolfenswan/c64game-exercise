
using System;
using UnityEngine;

public class CoinCollectState : CoinState 
{

    public CoinCollectState(CoinStateID id, CoinController entity, int animationHash) : base(id, entity, animationHash){}

    float _yForce;

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _yForce = _entity.Data.CollectUpwardsStrength; // TODO move to constructor once finalized
    }
    public override Enum Tick()
    {   
        _entity.MoveStep(0f, _yForce * Time.deltaTime);
        _yForce += _gravityVector.y * Time.deltaTime;

        return null;
    }
}