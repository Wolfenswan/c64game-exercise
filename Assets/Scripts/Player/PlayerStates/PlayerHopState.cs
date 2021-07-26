using System;
using UnityEngine;

// PlayerHopState is a variation of jump, used for example if the player is bumped from below by another player or when another player activates POW
public class PlayerHopState : PlayerJumpState 
{   
    public PlayerHopState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash){}

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _velocityVector *= _player.Data.HopModifier;
    }
}