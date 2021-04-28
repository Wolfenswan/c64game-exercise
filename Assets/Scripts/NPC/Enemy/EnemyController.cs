using System;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour 
{   
    public event Action EnemyFlipEvent;

    [SerializeField] EnemyData _data;
    [SerializeField] CollisionController _collisionController;
    [NonSerialized] public int Facing = 1;
    
    GFXController _gfxController;
    StateMachine _stateMachine;
    Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();
    
    // Public accessor to communicate if the current state gives points when flippings.
    List<EnemyStateID> _givePointsInStates = new List<EnemyStateID>{EnemyStateID.MOVE, EnemyStateID.TURN};
    public bool GivePointsOnFlip{get=>_givePointsInStates.Contains(CurrentStateID);}
    
    public EnemyData Data{get => _data;}
    public EnemyStateID CurrentStateID{get => (EnemyStateID) _stateMachine.CurrentState.ID;}
    public bool IsFlipped{get => CurrentStateID == EnemyStateID.FLIPPED;}
    public bool IsTouchingGround{get=>_collisions[CollisionType.GROUND];}
    public bool IsFacingOtherEntity{get=>(Facing == 1 && _collisions[CollisionType.ENTITY_RIGHT]) || (Facing == -1 && _collisions[CollisionType.ENTITY_LEFT]);}

    void Awake() 
    {   
        var states = new List<State> 
        {
            new EnemyMoveState(EnemyStateID.MOVE, this, 0),
            new EnemyFallState(EnemyStateID.FALL, this, 1),
            new EnemyFlippedState(EnemyStateID.FLIPPED, this, 2, 0),
            new EnemyTurnState(EnemyStateID.TURN, this, 3)
        };
        _stateMachine = new StateMachine(states, EnemyStateID.MOVE);

        _gfxController = GetComponent<GFXController>();
    }

    void OnEnable() 
    {
    }

    void OnDisable() 
    {
    }

    void Update() 
    {
        _collisions = _collisionController.UpdateCollisions();
        _stateMachine.Tick(TickMode.UPDATE);
    }

    public void MoveStep(float x, float y)
    {

    }

    public void IncreaseAnger()
    {   
        // three anger levels: normal, blue color, red color
        // increase movement speed
        // change sprite color
    }

    public virtual void OnFlip()
    {   
        // Replace with more advanced OnFlip in more complex enemies; e.g. flipping to different move state instead of flip on back            
        EnemyFlipEvent?.Invoke();
    }

    public void OnDead()
    {
        // TODO force to die state (if it needs own state?)
    }
    
}