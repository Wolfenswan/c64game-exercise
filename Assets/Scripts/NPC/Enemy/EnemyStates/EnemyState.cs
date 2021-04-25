using System;

public abstract class EnemyState : State
{
    protected readonly EnemyController _entity;
    protected readonly GFXController _gfxController;
    private readonly int? _animationHash;

    public EnemyState(EnemyStateID id, EnemyController entity, int? baseAnimationHash = null) 
    {   
        ID = id;
        _entity = entity;
        _gfxController = _entity.gameObject.GetComponent<GFXController>();
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