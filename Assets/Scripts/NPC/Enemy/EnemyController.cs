using System;
using System.Collections.Generic;
using UnityEngine;

// TODO enemy speeds
// Only top speed is equally as fast as player (for basic enemy)

public class EnemyController : NPCController
{   
    [SerializeField] EnemyData _data;
    [SerializeField] CollisionController _collisionController;
    
    StateMachine _stateMachine;
    Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();

    int _currentAngerLevel = 1;
    
    // Public accessor to communicate if the current state gives points when flippings.
    List<EnemyStateID> _givePointsInStates = new List<EnemyStateID>{EnemyStateID.MOVE, EnemyStateID.TURN};
    public bool GivePointsOnFlip{get=>_givePointsInStates.Contains(CurrentStateID);}
    
    public EnemyData Data{get => _data;}
    public int FlipPoints{get => _data.PointsOnFlip;}
    public int KillPoints{get => _data.PointsOnKill;}
    public EnemyStateID CurrentStateID{get => (EnemyStateID) _stateMachine.CurrentState.ID;}
    public bool IsFlipped{get => CurrentStateID == EnemyStateID.FLIPPED;}
    public bool IsTouchingGround{get=>_collisions[CollisionType.GROUND];}
    public bool IsTouchingEntityFront{get=>(Facing == EntityFacing.RIGHT && _collisions[CollisionType.ENTITY_RIGHT]) || (Facing == EntityFacing.LEFT && _collisions[CollisionType.ENTITY_LEFT]);}

    protected override void Awake() 
    {   
        base.Awake();
        var states = new List<State> 
        {
            new EnemyMoveState(EnemyStateID.MOVE, this, 0),
            new EnemyFallState(EnemyStateID.FALL, this, 1),
            new EnemyFlippedState(EnemyStateID.FLIPPED, this, 2, 0),
            new EnemyTurnState(EnemyStateID.TURN, this, 3)
        };
        _stateMachine = new StateMachine(states, EnemyStateID.MOVE);
    }

    void Update() 
    {
        _collisions = _collisionController.UpdateCollisions();
        _stateMachine.Tick(TickMode.UPDATE);
    }

    public void IncreaseAnger(int value)
    {   
        _currentAngerLevel = Mathf.Clamp(_currentAngerLevel + value, 1, _data.MaxAnger);
        // default three anger levels: normal, blue color, red color
        // increase movement speed
        // change sprite color
    }

    public void OnDead()
    {
        // TODO force to die state (if it needs own state?)
    }
    
}