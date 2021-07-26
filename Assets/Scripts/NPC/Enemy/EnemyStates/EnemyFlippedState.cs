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
    float _flippingBackStartTime;
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
        _introAnimDone = true;
        _exitAnimDone = false;
        _flippingBack = false;

        _velocityVector = _entity.Data.FlipVector; // TODO move to constructor once finalized

        // Determine whether to flip upwards or in an angle.
        // This is not the most precise method, as it uses the position of the player that prompted the flip rather than the actual contact point, but it's serviceable for the purpose
        SetFlipVectorAngle(ref _velocityVector);

        _gfxController.AnimationFinishedEvent += AnimFinished;
    }

    public override Enum Tick()
    {   
        // At the beginning of the state, describe the arc for the flipped enemy, until it touches ground again and the intro animation has played
        if(!_introAnimDone || _runTime < 0.05f || !_entity.IsTouchingGround)
        {
            MoveFlipArcStep();
        }
        else if (!_returningToMove && _introAnimDone && _entity.IsTouchingGround && !_flippingBack && !_exitAnimDone)
        {
            CanBeFlipped = true; // After the initial arc, the enemy can be flipped again, unless it is registered as flipping back already

            // if(!_flippingBack) // As long as the enemy is idle in the flipped state //* Commented out because probably unnecessary
            // {
            // Receiving the signal to flip prompts the reversal of the state
            if (_doFlip) StartTransitionToFlipBack();

            // If the enemy has not been flipped again or killed, they'll return to the move state after a certain time has passed
            // If this time has passed a special animation is playedspecial animation
            // They can still be flipped during this animation, but it is considered as flipping back to the move state
            if (_runTime >= _entity.Data.FlippedDuration && !_returningToMove) StartOwnTransitionToMove();
            // }         
        } else if(_flippingBack) // If the enemy has been flipped again (through jump from below or POW) they describe the arc again and will switch back to move after a certain time has passed
        {
            MoveFlipArcStep();

            if(Time.time - _flippingBackStartTime > 0.1f && _entity.IsTouchingGround)
                return EnemyStateID.MOVE;
        } else if (_returningToMove && _exitAnimDone && !_flippingBack) // If the enemy has flipped back on their own, they will increase their speed(=amger) and return to the move state
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

    void MoveFlipArcStep()
    {           
        _entity.MoveStep(_velocityVector.x, _velocityVector.y, applyDeltaTime:true);
        _velocityVector += _gravityVector * Time.deltaTime;
    }

    void SetFlipVectorAngle(ref Vector2 flipVector)
    {
        var angle = Vector2Utilities.GetDegreesBetweenVectors(_entity.EventContactVector, _entity.Pos);
        if(85 <= angle && angle <= 95) 
            flipVector.x = 0;
        else if (angle > 95)
            flipVector.x *= -1;
    }

    void StartTransitionToFlipBack()
    {
        _flippingBackStartTime = Time.time;
        _flippingBack = true;
        _velocityVector = _entity.Data.FlipVector; // The flip-back is always arced
        SetFlipVectorAngle(ref _velocityVector);
        // TODO Freeze all Animation -> https://docs.unity3d.com/ScriptReference/Animator-speed.html to 0
    }

    void StartOwnTransitionToMove()
    {
        if(_entity.Debugging)
            Debug.Log($"{_entity} returning to move on its own.");

        _returningToMove = true;
        _gfxController.ChangeAnimation((int) _exitAnimation);
    }

    void AnimFinished(int hash)
    {   
        if (hash == _animationHash) _introAnimDone = true;
        if (hash == _exitAnimation) _exitAnimDone = true;
    }
}