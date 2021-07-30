using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

// TODO enemy speeds
// Only top speed is equally as fast as player (for basic enemy)

public class EnemyController : NPCController
{   
    public event Action EnemyFlipEvent; // Listened to by the individual states.

    [SerializeField] EnemyData _data;
    
    EnemyAngerLevel _currentAngerLevel;
    
    public EnemyStateID CurrentEnemyStateID{get => (EnemyStateID) CurrentStateID;}
    public bool GivePointsOnFlip{get=> ((EnemyState) CurrentState).GivePointsOnFlip;}
    public bool CanBeFlipped{get => ((EnemyState) CurrentState).CanBeFlipped;}
    public bool CanKillPlayer{get => !IsDie && !IsTouchingSpawnOrExit;} // Enemies should not kill the player while they are spawning, exiting the stage or are already dead
    
    public EnemyData Data{get => _data;}
    public float MoveSpeed{get => _currentAngerLevel.Speed;}
    public int FlipPoints{get => _data.PointsOnFlip;}
    public int KillPoints{get => _data.PointsOnKill;}
    public Vector2 EventContactVector{get;private set;} = Vector2.zero; // Used by the flip & kill states to determine where the contact came from and apply the according arc to their movement

    #region state shorthands
    public bool IsDie{get => CurrentEnemyStateID == EnemyStateID.DIE;}
    public bool IsFlipped{get => CurrentEnemyStateID == EnemyStateID.FLIPPED;}
    #endregion

    struct AnimationHash
    {
        public static readonly int Move = AtH("Move");
        public static readonly int Turn = AtH("Turn");
        public static readonly int FlippedIn = AtH("FlippedIn");
        public static readonly int FlippedExit = AtH("FlippedExit");
        public static readonly int Die = AtH("Die");

        static int AtH(string s) => Animator.StringToHash(s);
    }

    protected override void Start() 
    {
        base.Start();
        SetAngerLevel(1);
    }

    void OnEnable() 
    {
        _gfxController.AnimationFinishedEvent += GFX_AnimationFinishedEvent;
    }

    protected override void OnDisable() 
    {
        base.OnDisable();
        _gfxController.AnimationFinishedEvent -= GFX_AnimationFinishedEvent;
    }

    protected override void InitializeStateMachine()
    {
        var states = new List<State> 
        {
            new EnemyMoveState(EnemyStateID.MOVE, this, AnimationHash.Move),
            new EnemyTurnState(EnemyStateID.TURN, this, AnimationHash.Turn),
            new EnemyFallState(EnemyStateID.FALL, this),
            new EnemyFlippedState(EnemyStateID.FLIPPED, this, AnimationHash.FlippedIn, AnimationHash.FlippedExit),
            new EnemyDieState(EnemyStateID.DIE, this, AnimationHash.Die)
        };
        _stateMachine = new StateMachine(states, EnemyStateID.MOVE);
    }

    public void IncreaseAnger(int byValue) => SetAngerLevel(_currentAngerLevel.ID + byValue);

    void SetAngerLevel(int level)
    {
        if (_data.AngerLevels.Exists(aL => aL.ID == level))
            _currentAngerLevel = _data.AngerLevels.Single(aL => aL.ID == level);
        else // if the level does not exist, set to the highest state
            _currentAngerLevel = _data.AngerLevels[_data.AngerLevels.Count - 1];

        _gfxController.SetSpriteColor(_currentAngerLevel.Color);
    }

    public virtual void OnFlip(Vector2 contactPoint)
    {   
        // Replaced by more advanced implementations in more complex enemies; e.g. flipping to different move state instead of flip on back
        EventContactVector = contactPoint;            
        EnemyFlipEvent?.Invoke();
    }

    public void OnDead(Vector2 contactPoint)
    {  
        EventContactVector = contactPoint;
        _stateMachine.ForceState(EnemyStateID.DIE);
    }

    public override void Delete()
    {
        if (!IsDie) _stateMachine.ForceState(EnemyStateID.DIE);
    }

    void GFX_AnimationFinishedEvent(int animationHash)
    {
        if (animationHash == AnimationHash.Die) Destroy(gameObject);
    }
}