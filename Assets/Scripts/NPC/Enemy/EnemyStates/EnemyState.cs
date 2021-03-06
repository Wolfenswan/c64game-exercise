using System;
using UnityEngine;

public abstract class EnemyState : State
{
    protected readonly EnemyController _entity;
    protected readonly GFXController _gfxController;
    protected readonly int? _animationHash;
    protected bool _doFlip = false;
    protected Vector2 _flipContactPoint;
    protected Vector2 _gravityVector;

    public bool CanBeFlipped{get; protected set;} 
    public bool GivePointsOnFlip{get; protected set;} 

    public EnemyState(EnemyStateID id, EnemyController entity, int? baseAnimationHash = null) 
    {   
        ID = id;
        _entity = entity;
        _gfxController = _entity.gameObject.GetComponent<GFXController>();
        _animationHash = baseAnimationHash;
    }

    public override void OnEnter(Enum fromState) 
    {
        base.OnEnter(fromState);

        _gravityVector = GameManager.Instance.Data.GravityVector; // TODO move to constructor once final

        _doFlip = false;
        _flipContactPoint = Vector2.zero;
        _entity.EnemyFlipEvent += Entity_NPCFlipEvent;

        if (_animationHash != null)
            _gfxController.ChangeAnimation((int) _animationHash);    
    }

    public override void OnExit(Enum toState) 
    {
        _entity.EnemyFlipEvent -= Entity_NPCFlipEvent;
    }

    void Entity_NPCFlipEvent()
    {
        _doFlip = true;
    }
}