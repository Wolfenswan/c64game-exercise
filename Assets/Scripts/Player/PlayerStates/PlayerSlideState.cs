using System;
using UnityEngine;

public class PlayerSlideState : PlayerState 
{
    public PlayerSlideState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    float _moveTimer;

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);
        _moveTimer = 0;
    }

    public override Enum Tick()
    {
        var maxDuration = _player.Data.SlideDuration; // TODO once the value is final, move to constructor or declare as field
        var moveSpeed = _player.Data.MoveSpeed; // TODO once the value is final, move to constructor or declare as field

        if (!_player.IsTouchingGround) // TODO use gravity vector / dedicated state?
        {   
            _player.MoveStep(moveSpeed * Time.deltaTime * (int) _player.Facing, _gravityVector.y * Time.deltaTime);
        } else
        {
            _moveTimer += Time.deltaTime; // Not using _runTime, as fall should be excluded
            
            if (_doJump)
                return PlayerStateID.JUMP;

            if (_moveTimer >= maxDuration)
                return PlayerStateID.IDLE;
                
            _player.MoveStep((moveSpeed * Time.deltaTime) * (int) _player.Facing, 0f);
        }

        return null;
    }
}