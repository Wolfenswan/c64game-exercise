using System;

public abstract class PlayerState : State
{   
    protected readonly PlayerController _player;
    protected readonly GFXController _gfxController;
    private readonly int? _animationHash;

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

        if (_animationHash != null)
            _gfxController.ChangeAnimation((int) _animationHash);
            
    }
    
    public override void OnExit(Enum fromState) {}
}