using System;
using UnityEngine;

public class PlayerMoveState : PlayerState 
{   
    float _moveTimer;

    public PlayerMoveState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}
    
    public override void OnEnter(Enum toState)
    {
        base.OnEnter(toState);

        _moveTimer = 0f;
    }

    public override Enum Tick()
    {
        var direction = _player.MovementInput;
        var timeToSlide = _player.Data.TimeToSlide; // TODO once the value is final, move to constructor or declare as field
        var moveSpeed = _player.Data.MoveSpeed; // TODO once the value is final, move to constructor or declare as field
    
        if(!_player.IsTouchingGround)
        {   
            _player.MoveStep(moveSpeed * Time.deltaTime, _player.Data.GravityVector.y * Time.deltaTime);
            _moveTimer = 0; // Player should not slide after falling from a move state
        } else
        {   
            _moveTimer += Time.deltaTime; // Not using _runTime, as fall should be excluded
            
            if (_executeJump)
                return PlayerStateID.JUMP;
            
            if (direction == 0 && _moveTimer >= timeToSlide)
                return PlayerStateID.SLIDE;
            
            if (direction == 0)
                return PlayerStateID.IDLE;
            
            if (direction != 0)
            {   
                _player.UpdateFacing(direction);
                _player.MoveStep((moveSpeed * Time.deltaTime) * direction, 0f);
            }
        }

        return null;
    }
}