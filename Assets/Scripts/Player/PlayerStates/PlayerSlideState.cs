using System;
using UnityEngine;

public class PlayerSlideState : PlayerState 
{
    public PlayerSlideState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    Vector3 _moveVector = new Vector3(0,0,0);
    float _moveTimer;

    public override void OnEnter(Enum toState)
    {
        base.OnEnter(toState);
        _moveTimer = 0;
    }

    public override Enum Tick()
    {
        var maxDuration = _player.Data.SlideDuration; // TODO once the value is final, move to constructor or declare as field
        var moveSpeed = _player.Data.MoveSpeed; // TODO once the value is final, move to constructor or declare as field; ALSO - might use the same as move speed?
        // if slide time is over; or use distance?

        if (!_player.IsTouchingGround)
        {
            _moveVector.Set((moveSpeed * Time.deltaTime) * _player.Facing, (moveSpeed * Time.deltaTime) * -1, 0f);
            _player.transform.position += _moveVector;
        } else 
        {
            _moveTimer += Time.deltaTime; // Not using _runTime, as fall should be excluded
            
            if (_player.JumpInput && _runTime > 0.05f) // Odd bug with the input system that registers jump input for a bit longer than it should)
                return PlayerStateID.JUMP;

            if (_moveTimer >= maxDuration)
                return PlayerStateID.IDLE;

            _moveVector.x = (moveSpeed * Time.deltaTime) * _player.Facing;
            _player.transform.position += _moveVector;
        }

        return null;
    }
}