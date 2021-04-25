using System;
using UnityEngine;
using NEINGames.BezierCurve;

public class PlayerJumpState : PlayerState 
{   
    

    public PlayerJumpState(PlayerStateID id, PlayerController player, int animationHash) : base(id, player, animationHash) 
    {
        
    }
    Vector2 _jumpPower;
    Vector2 _moveVector;
    BezierCurve2D _arc;
    Vector2 _p1;
    Vector2 _p2;
    float _t;

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _t = 0;
        _moveVector = Vector2.zero;

        _jumpPower = _player.Data.JumpPower; // TODO once the value is final, move to constructor or declare as field
        if (_player.MovementInput != 0 || (PlayerStateID) fromState == PlayerStateID.SLIDE)
        {   
            var dir = _player.Facing;
            _p1.Set(_player.Pos.x + _jumpPower.x/2 * dir, _player.Pos.y + _jumpPower.y);
            _p2.Set(_player.Pos.x + _jumpPower.x * dir, _player.Pos.y);
        } else {
            _p1.Set(_player.Pos.x,_player.Pos.y + _jumpPower.y);
            _p2.Set(_player.Pos.x,_player.Pos.y);
            
        }
        _arc = new BezierCurve2D(_player.Pos, _p1, _p2);
    }

    public override Enum Tick()
    {   
        _t += _player.Data.JumpSpeed * Time.deltaTime; // TODO once the value is final, move to constructor or declare as field
        if (_player.Debugging)
            _arc.Draw(0.1f,0.1f);
        _moveVector = _arc.GetPoint(_t);
        
        if (_player.IsTouchingCeiling && _moveVector.y > _player.Pos.y)
            _moveVector.y = _player.Pos.y;

        _player.transform.position = _moveVector;
        
        // Checking against _t to make sure the jump always tries to reach its peak
        if ((_player.IsTouchingGround && _t >= 0.49) || Mathf.Approximately(Vector2.Distance(_player.Pos, _arc.EndPoint),0))
            return PlayerStateID.IDLE;

        return null;
    }
}