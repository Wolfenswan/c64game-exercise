using System;
using UnityEngine;

public class PlayerIdleState : PlayerState 
{
    public PlayerIdleState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    Vector3 _moveVector;

    public override Enum Tick()
    {   
        if (_player.IsPlayerFrozen)
            return null;

        if(!_player.IsTouchingGround) // TODO dedicated state?
        {  
            _player.MoveStep(0f, _player.Data.GravityVector.y * Time.deltaTime);
        } else {
            if (_player.MovementInput != 0)
                return PlayerStateID.MOVE;
            
            // if (_player.JumpInput && _runTime > 0.05f) // Odd bug with the input system that registers jump input for a bit longer than it should)
            //     return PlayerStateID.JUMP;

            if (_executeJump)
                return PlayerStateID.JUMP;
        }

        return null;
    }
}