using System;
using UnityEngine;

public class PlayerDieState : PlayerState 
{   
    public PlayerDieState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    public override Enum Tick()
    {   
        if (_animFinished)
        {
            var speed = _gravityVector.y * Time.deltaTime;
            _player.MoveStep(0f, speed);
        }

        return null;
    }
}
