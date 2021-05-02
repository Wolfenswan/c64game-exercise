using System;
using UnityEngine;

public abstract class CoinState : State
{
    protected readonly CoinController _entity;
    protected readonly GFXController _gfxController;
    protected readonly int? _animationHash;
    protected Vector2 _gravityVector;

    public CoinState(CoinStateID id, CoinController entity, int? baseAnimationHash = null) 
    {   
        ID = id;
        _entity = entity;
        _gfxController = _entity.gameObject.GetComponent<GFXController>();
        _animationHash = baseAnimationHash;
    }

    public override void OnEnter(Enum fromState) 
    {
        base.OnEnter(fromState);

        _gravityVector = GameManager.Instance.Data.GravityVector; // TODO move to constructor once final

        if (_animationHash != null)
            _gfxController.ChangeAnimation((int) _animationHash);    
    }

    public override void OnExit(Enum toState) 
    {
    }
}