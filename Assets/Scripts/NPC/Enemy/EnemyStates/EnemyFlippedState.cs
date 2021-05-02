using UnityEngine;
using System;
using NEINGames.Utilities;

public class EnemyFlippedState : EnemyState 
{   
    bool _introAnimDone;
    int _exitAnimation;
    bool _exitAnimDone;
    bool _returningToMove;
    bool _flippingBack;
    float _flippingBackTimer;
    Vector2 _velocityVector;
    
    public EnemyFlippedState(EnemyStateID id, EnemyController entity, int animationHash, int exitAnimationHash) : base(id, entity, animationHash) 
    {
        _exitAnimation = exitAnimationHash;
        GivePointsOnFlip = false;
    }

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        CanBeFlipped = false;

        _returningToMove = false;
        _introAnimDone = true; //! DEBUG
        _exitAnimDone = false;
        _flippingBack = false;

        _velocityVector = _entity.Data.FlipVector; // TODO move to constructor once finalized

        // Determine whether to flip upwards or in an angle.
        // This is not the most precise method, as it uses the position of the player that prompted the flip rather than the actual contact point, but it's serviceable for the purpose
        var angle = Vector2Utilities.GetDegreesBetweenVectors(_entity.EventContactVector, _entity.Pos);
        if(85 <= angle && angle <= 95) 
            _velocityVector.x = 0;
        else if (angle > 95)
            _velocityVector.x *= -1;

        _gfxController.AnimationFinishedEvent += AnimFinished;
    }

    public override Enum Tick()
    {   
        // At the beginning of the state, describe the arc for the flipped enemy, until it touches ground again and the intro animation has played
        if(!_introAnimDone || _runTime < 0.05f || !_entity.IsTouchingGround)
        {
            MoveFlipArc();
        }
        else if (_introAnimDone && _entity.IsTouchingGround && !_flippingBack && !_exitAnimDone)
        {
            CanBeFlipped = true; // After the initial arc, the enemy can be flipped again, unless it is registered as flipping back already

            if(!_flippingBack) // As long as the enemy is idle in the flipped state
            {
                if (_doFlip) // Receiving the signal to flip prompts the reversal of the state.
                {   
                    _flippingBackTimer = Time.time;
                    _flippingBack = true;
                    _velocityVector = _entity.Data.FlipVector; // The flip-back is always arced
                    // TODO Freeze all Animation -> https://docs.unity3d.com/ScriptReference/Animator-speed.html to 0
                }

                // If the enemy has not been flipped again or killed, they'll return to the move state after a special animation
                // They can still be flipped during this animation, but it is considered as flipping back to the move state
                if (_runTime >= _entity.Data.FlippedDuration && !_returningToMove) 
                {   
                    _returningToMove = true;
                    _gfxController.ChangeAnimation((int) _exitAnimation);
                }
            }         
        } else if(_flippingBack)
        {
            MoveFlipArc();

            if(Time.time - _flippingBackTimer > 0.1f && _entity.IsTouchingGround)
                return EnemyStateID.MOVE;
        } else if (_exitAnimDone && !_flippingBack)
        {
            _entity.IncreaseAnger(1);
            return EnemyStateID.MOVE;
        }   
        return null;
    }

    public override void OnExit(Enum toState)
    {
        _gfxController.AnimationFinishedEvent -= AnimFinished;
    }

    void MoveFlipArc()
    {   
        var speed = _entity.Data.MoveSpeed * Time.deltaTime; // TODO add different speed as required
        var newPos = (Vector2) _entity.transform.position + _velocityVector * speed;
        
        _entity.transform.position = newPos;
        _velocityVector += _gravityVector * speed;
    }

    void AnimFinished(int hash)
    {   
        if (hash == _animationHash) _introAnimDone = true;
        if (hash == _exitAnimation) _exitAnimDone = true;
    }
}