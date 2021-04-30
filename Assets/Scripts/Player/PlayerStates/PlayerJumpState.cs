using System;
using UnityEngine;

public class PlayerJumpState : PlayerState 
{   
    public PlayerJumpState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash){}

    Vector2 _velocityVector = new Vector2(0,0);
    bool _wasBounced;

    public void ReverseJumpDirection() 
    {   
        if (_wasBounced)
            return;

        _velocityVector.Set(_velocityVector.x * -1 , _velocityVector.y);

        _wasBounced = true;
    }

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _wasBounced = false;

        _velocityVector = _player.Data.JumpVector; // TODO move to constructor / field once finalized
        _velocityVector.x *= (int) _player.Facing;

        //* NOTE: if bouncing from other player below the default x-vector might be a bit too strong. In that case check individually and apply a low x (maybe just velV.x * 0.5?)
        if (!_player.IsTouchingOtherPlayerDown && ((PlayerStateID) fromState == PlayerStateID.IDLE || (PlayerStateID) fromState == PlayerStateID.SPAWN) && _player.MovementInput == 0)
            _velocityVector.x = 0f;
    }

    public override Enum Tick()
    {   
        var speed = _player.Data.JumpSpeed * Time.deltaTime; // TODO move to constructor / field once finalized
        var newPos = _player.Pos + _velocityVector * speed;  

        // Prevent player from jumping through the ceiling by sliding them along beneath it
        if (_player.IsTouchingCeiling && newPos.y > _player.Pos.y) newPos.y = _player.Pos.y;

        // Make sure the jump always tries to reach its peak before returning to a grounded state
        if (_runTime > 0.15f && _player.IsTouchingGround && newPos.y < _player.Pos.y)
        {   
            if (_velocityVector.x == 0)
                return PlayerStateID.IDLE;
            else
            {
                if (_wasBounced) _player.ReverseFacing();
                return PlayerStateID.SLIDE;
            }
                
        }

        _player.transform.position = newPos;
        _velocityVector += _gravityVector * speed;

        return null;
    }
}