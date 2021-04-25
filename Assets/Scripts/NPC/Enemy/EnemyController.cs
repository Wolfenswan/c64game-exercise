using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour 
{   
    public event Action<EnemyController> EnemyDeadEvent;    
    [SerializeField] EnemyData _data;
    [SerializeField] CollisionController _collisionController;

    [NonSerialized] public int Facing = 1;
    
    GFXController _gfxController;
    StateMachine _stateMachine;
    Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();

    public EnemyData Data{get => _data;}
    public bool CanBeFlipped{get; private set;}
    public bool IsFlipped{get; private set;}
    public bool IsTouchingGround{get=>_collisions[CollisionType.GROUND];}
    public bool IsTouchingOtherEntity{get=>_collisions[CollisionType.ENTITY];}

    //bool isGrounded

    // Update
    // Tick State

    void Awake() 
    {   
        var states = new List<State> 
        {
            new EnemyMoveState(EnemyStateID.MOVE, this, 0),
            new EnemyFallState(EnemyStateID.FALL, this, 1),
            new EnemyTurnState(EnemyStateID.TURN, this, 2)
        };
        _stateMachine = new StateMachine(states, EnemyStateID.MOVE);

        _gfxController = GetComponent<GFXController>();
    }

    void OnEnable() 
    {
        PlayerController.EnemyFlipEvent += PlayerController_EnemyFlipEvent;
        PlayerController.EnemyKillEvent += PlayerController_EnemyKillEvent;
    }

    void OnDisable() 
    {
        PlayerController.EnemyFlipEvent -= PlayerController_EnemyFlipEvent;
        PlayerController.EnemyKillEvent -= PlayerController_EnemyKillEvent;
    }

    void Update() 
    {
        _collisions = _collisionController.UpdateCollisions(Facing);
        _stateMachine.Tick(TickMode.UPDATE);
    }

    void OnFlip()
    {   
        CanBeFlipped = false;
        // Enforce Flipped State; maybe check for isFlipped and then have to States: FlipForward, FlipBackward
        // Or easier: single flip state but with two additional animations: FlipForward, FlipBackward. Decide in OnEnter() which to play
        // StartCoroutine or something? Or just handle everything in the flipped state?
    }

    void OnKilled()
    {
        EnemyDeadEvent?.Invoke(this);
        // Enforce DeadState
        // Disable gameObject after some time
    }

    void PlayerController_EnemyFlipEvent(EnemyController enemyHit)
    {
        if (this != enemyHit)
            return;

        if (CanBeFlipped) // Probably redundant with check in PlayerController
            OnFlip();
    }

    void PlayerController_EnemyKillEvent(EnemyController enemyHit)
    {   
        if (this != enemyHit)
            return;

        OnKilled();
    }
    
}