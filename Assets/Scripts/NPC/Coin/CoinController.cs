
using UnityEngine;
using System.Collections.Generic;

public class CoinController : NPCController
{
    [SerializeField] CoinData _data;

    public CoinData Data{get=>_data;}
    public int Points{get=>_data.Points;}

    public CoinState CurrentState{get=> (CoinState) _stateMachine.CurrentState;}
    public CoinStateID CurrentStateID{get => (CoinStateID) CurrentState.ID;}
    public override string CurrentStateName{get => CurrentState.ToString();}
    public bool IsCollect{get => CurrentStateID == CoinStateID.COLLECT;}

    public bool CanBeCollected{get=> !IsCollect;}

    StateMachine _stateMachine;

    struct AnimationHash
    {
        public static readonly int Move = AtH("Move");
        public static readonly int Collect = AtH("Collect");

        static int AtH(string s) => Animator.StringToHash(s);
    }

    protected override void Awake()
    {   
        base.Awake();
        var states = new List<State> 
        {
            new CoinMoveState(CoinStateID.MOVE, this, AnimationHash.Move), //! TODO Add AnimationHashes
            new CoinCollectState(CoinStateID.COLLECT, this, AnimationHash.Collect),
        };
        _stateMachine = new StateMachine(states, CoinStateID.MOVE);
    }

    void OnEnable()
    {
        _gfxController.AnimationFinishedEvent += GFX_AnimationFinishedEvent;
    }

    void OnDisable() 
    {
        _gfxController.AnimationFinishedEvent -= GFX_AnimationFinishedEvent;
    }

    protected override void Update()
    {
        base.Update();

        _collisions = _collisionController.UpdateCollisions();
        _stateMachine.Tick(TickMode.UPDATE);
    }

    public void OnCollect()
    {
        _stateMachine.ForceState(CoinStateID.COLLECT);
    }

    public override void Delete()
    {
        if (!IsCollect) OnCollect();
    }

    void GFX_AnimationFinishedEvent(int animHash)
    {
        if (animHash == AnimationHash.Collect) Destroy(gameObject); // TODO or destroy entirely?
    }
}