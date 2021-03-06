using System;
using UnityEngine;

public abstract class FlameState : State
{
    protected readonly FlameController _entity;
    protected readonly GFXController _gfxController;
    protected readonly int? _animationHash;

    public bool DoesHurt{get;private set;}

    public FlameState(FlameStateID id, FlameController entity, int? baseAnimationHash = null, bool doesHurt = false) 
    {   
        ID = id;
        _entity = entity;
        _gfxController = _entity.gameObject.GetComponent<GFXController>();
        _animationHash = baseAnimationHash;
        DoesHurt = doesHurt;
    }

    public override void OnEnter(Enum fromState) 
    {
        base.OnEnter(fromState);

        if (_animationHash != null)
            _gfxController.ChangeAnimation((int) _animationHash);
        
        _gfxController.AnimationFinishedEvent += GFX_AnimationFinishedEvent;           
    }

    public override void OnExit(Enum toState) 
    {
        _gfxController.AnimationFinishedEvent -= GFX_AnimationFinishedEvent;
    }

    protected virtual void GFX_AnimationFinishedEvent(int animHash){}
}