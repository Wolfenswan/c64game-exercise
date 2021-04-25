using System;
using UnityEngine;

public class PlayerMoveState : PlayerState 
{   
    Vector3 _moveVector = new Vector3(0,0,0);
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
            _moveVector.Set((moveSpeed * Time.deltaTime) * _player.Facing, (moveSpeed * Time.deltaTime) * -1, 0f);
            _player.transform.position += _moveVector;
            _moveTimer = 0; // TODO test if desireable
        } else
        {   
            _moveTimer += Time.deltaTime; // Not using _runTime, as fall should be excluded
            
            if (_player.JumpInput && _runTime > 0.05f) // Odd bug with the input system that registers jump input for a bit longer than it should
                return PlayerStateID.JUMP;
                
            
            if (direction == 0 && _moveTimer >= timeToSlide)
                return PlayerStateID.SLIDE;
            
            if (direction == 0)
                return PlayerStateID.IDLE;
            
            if (direction != 0)
            {   
                _moveVector.Set((moveSpeed * Time.deltaTime) * direction, 0f, 0f);
                _player.transform.position += _moveVector;
            }
        }

        return null;
    }

}