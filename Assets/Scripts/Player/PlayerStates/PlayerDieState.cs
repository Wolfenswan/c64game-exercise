using System;
using UnityEngine;
using NEINGames.BezierCurve;

public class PlayerDieState : PlayerState 
{   
    BezierCurve2D _path;
    float _t;

    public PlayerDieState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) {}

    // Entire purpose is to send signal once the animation is done (or just wait for a given time set in data; can then also be used in the Respawn Coroutine)
    // Animation should be fairly static, then use a BezierCurve2D as a flat line
    // Then shift back to IDLE if lives > 0. Otherwise simply remain in this state.

    public override void OnEnter(Enum toState)
    {
        base.OnEnter(toState);

        var playerPos = (Vector2) _player.transform.position;
        // TODO: easier in the original: animation plays, then fall down
        // Aber: könnte das als fancy effekt machen :)
        var p1 = new Vector2(playerPos.x, playerPos.y + 10f);
        var p2 = new Vector2(playerPos.x, playerPos.y - 10f);
        _path = new BezierCurve2D(p1, p2);
        _t = 0;
    }

    

}
