using UnityEngine;
using System;

public class EnemyFlippedState : EnemyState 
{   
    int _exitAnimation;
    bool _exitAnimDone;
    bool _returningToMove;
    bool _flippingBack;
    
    public EnemyFlippedState(EnemyStateID id, EnemyController entity, int animationHash, int exitAnimationHash) : base(id, entity, animationHash) 
    {
        _exitAnimation = exitAnimationHash;
    }

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _returningToMove = false;
        _exitAnimDone = false;
        _flippingBack = false;

        _gfxController.AnimationFinishedEvent += AnimFinished;
    }

    public override Enum Tick()
    {
        if (_exitAnimDone && !_flippingBack)
        {
            _entity.IncreaseAnger(1);
            return EnemyStateID.MOVE;
        }

        if (_flippingBack)
        {
            // TODO add small arc to fly in facing direction until isTouchingGround
            // How to get the correct arc, up & away from the player? Probably best to pass impact point or angle through the event from PlayerController
            // Use CoRoutine?
            // Resume all animation
            return EnemyStateID.MOVE;
        }

        if(!_flippingBack)
        {
            if (_doFlip && _entity.IsTouchingGround)
            {   
                _flippingBack = true; // Use CoRoutine?
                // TODO Freeze all Animation -> https://docs.unity3d.com/ScriptReference/Animator-speed.html to 0
            }
        
            if (_runTime >= _entity.Data.FlippedDuration && !_returningToMove)
            {   
                _returningToMove = true;
                _gfxController.ChangeAnimation((int) _exitAnimation);
            }
        }

        return null;
    }

    public override void OnExit(Enum toState)
    {
        _gfxController.AnimationFinishedEvent -= AnimFinished;
    }

    void AnimFinished(int hash)
    {
        if (hash == _exitAnimation) _exitAnimDone = true;
    }
}