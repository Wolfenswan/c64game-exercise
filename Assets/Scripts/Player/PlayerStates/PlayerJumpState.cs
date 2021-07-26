using System;
using UnityEngine;

public class PlayerJumpState : PlayerState 
{   
    public PlayerJumpState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash){}

    protected Vector2 _velocityVector;
    bool _wasBounced;
    bool _hasPeaked;

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _wasBounced = false;
        _hasPeaked = false;

        _velocityVector = _player.Data.JumpForceVector;

        if ((PlayerStateID) fromState == PlayerStateID.IDLE || ((PlayerStateID) fromState == PlayerStateID.SPAWN && _player.MovementInput == 0))
            _velocityVector.x = 0f;
        else
            _velocityVector.x *= (int) _player.Facing;        
    }

    public override Enum Tick()
    {   
        if (_runTime > 0.05f && _player.IsTouchingGround && _hasPeaked)
        {   
            if (_velocityVector.x == 0)
                return PlayerStateID.IDLE;
            else
            {
                if (_wasBounced) _player.ReverseFacing(); // Bouncing from another player while mid-air does not reverse the facing during the jump, that's why it needs to abdjusted upon landing
                return PlayerStateID.SLIDE;
            }
        }
        
        var jumpVector = _velocityVector * Time.deltaTime;

        if (!_hasPeaked && (_player.IsTouchingCeiling || _player.IsTouchingEntityAbove || jumpVector.y <= 0)) _hasPeaked = true;

        if (_player.IsTouchingCeiling && jumpVector.y > 0) jumpVector.y = 0; // Prevent the player from jumping through platforms by resetting the y-force

        _player.MoveStep(jumpVector);

        // Apply gravity to reduce velocity next tick
        if (!_hasPeaked)
            _velocityVector += _gravityVector * Time.deltaTime;
        else // After the jump has peaked - either through application of the gravityVector or by hitting a ceiling, start apply a tiny bit of extra force each tick to accelerate the fall
            _velocityVector += _gravityVector * _player.Data.FallSpeedMultiplier * Time.deltaTime;

        return null;
    }

    public void ReverseJumpDirection() 
    {   
        if (_wasBounced)
            return;
            
        _velocityVector.Set(_velocityVector.x * -1 , _velocityVector.y);
        _wasBounced = true;
    }
}