using System;
using UnityEngine;

public class PlayerIdleState : PlayerState 
{
    public PlayerIdleState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    public override Enum Tick()
    {   
        if(!_player.IsTouchingGround) // TODO dedicated state?
        {  
            _player.MoveStep(0f, _gravityVector.y * Time.deltaTime);
        } else {
            if (_player.MovementInput != 0)
                return PlayerStateID.MOVE;
            
            // if (_player.JumpInput && _runTime > 0.05f) // Odd bug with the input system that registers jump input for a bit longer than it should)
            //     return PlayerStateID.JUMP;

            if (_doJump)
                return PlayerStateID.JUMP;
        }

        return null;
    }
}