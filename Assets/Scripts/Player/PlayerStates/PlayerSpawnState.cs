using System;
using UnityEngine;

public class PlayerSpawnState : PlayerState 
{
    public PlayerSpawnState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    public override Enum Tick()
    {   
        if (_player.MovementInput != 0)
        {   
            _player.UpdateFacing(_player.MovementInput);
            return PlayerStateID.MOVE;
        }
        
        if (_doJump)
            return PlayerStateID.JUMP;

        if(_runTime > _player.Data.RespawnGracePeriod)
            return PlayerStateID.IDLE;

        return null;
    }
}