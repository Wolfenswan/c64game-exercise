using System;
using UnityEngine;

public abstract class PlayerState : State
{   
    protected readonly PlayerController _player;
    protected readonly GFXController _gfxController;
    private readonly int? _animationHash;

    protected bool _doJump;
    protected bool _animFinished;
    protected private Vector2 _gravityVector;

    public PlayerState(PlayerStateID id, PlayerController player, int? baseAnimationHash = null) 
    {   
        ID = id;
        _player = player;
        _gfxController = _player.gameObject.GetComponent<GFXController>();
        _animationHash = baseAnimationHash;
    }

    public override void OnEnter(Enum fromState) 
    {
        base.OnEnter(fromState);

        _gravityVector = GameManager.Instance.Data.GravityVector; // TODO move to constructor once final

        _doJump = false;
        _player.JumpButtonEvent += Player_JumpEvent;
        _gfxController.AnimationFinishedEvent += GFX_AnimationFinishedEvent;

        if (_animationHash != null)
            _gfxController.ChangeAnimation((int) _animationHash);
            
    }
    
    public override void OnExit(Enum toState) 
    {
        _player.JumpButtonEvent -= Player_JumpEvent;
        _gfxController.AnimationFinishedEvent -= GFX_AnimationFinishedEvent;
    }

    void Player_JumpEvent() => _doJump = _player.IsTouchingGround || (PlayerStateID) ID == PlayerStateID.SPAWN;

    void GFX_AnimationFinishedEvent(int hash) => _animFinished = hash == _animationHash;
}