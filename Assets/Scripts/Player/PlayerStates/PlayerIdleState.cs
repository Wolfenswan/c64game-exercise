using System;
using UnityEngine;

public class PlayerIdleState : PlayerState 
{
    public PlayerIdleState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    Vector3 _moveVector;

    public override Enum Tick()
    {   
        if(!_player.IsTouchingGround)
        {  
            var moveSpeed = _player.Data.MoveSpeed;
            _moveVector.Set(0f, moveSpeed * Time.deltaTime * -1, 0f);
            _player.transform.position += _moveVector;
        } else {
            if (_player.MovementInput != 0)
                return PlayerStateID.MOVE;
            
            if (_player.JumpInput && _runTime > 0.05f) // Odd bug with the input system that registers jump input for a bit longer than it should)
                return PlayerStateID.JUMP;
        }

        return null;
    }

    public override Enum FixedTick()
    {
        return null;
    }
}