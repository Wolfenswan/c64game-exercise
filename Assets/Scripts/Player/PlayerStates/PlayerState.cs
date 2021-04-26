using System;
using UnityEngine;

public abstract class PlayerState : State
{   
    protected readonly PlayerController _player;
    protected readonly GFXController _gfxController;
    private readonly int? _animationHash;

    protected bool _executeJump;
    private Vector3 _fallVector;
    private Vector3 _moveVector;

    public PlayerState(PlayerStateID id, PlayerController player, int? baseAnimationHash = null) 
    {   
        ID = id;
        _player = player;
        _gfxController = _player.gameObject.GetComponent<GFXController>();
        _animationHash = baseAnimationHash;
    }

    public override void OnEnter(Enum toState) 
    {
        base.OnEnter(toState);

        _executeJump = false;
        _player.JumpEvent += Player_JumpEvent;

        if (_animationHash != null)
            _gfxController.ChangeAnimation((int) _animationHash);
            
    }
    
    public override void OnExit(Enum fromState) 
    {
        _player.JumpEvent -= Player_JumpEvent;
    }

    void Player_JumpEvent()
    {
        if(_player.IsTouchingGround) _executeJump = true;
    }
}