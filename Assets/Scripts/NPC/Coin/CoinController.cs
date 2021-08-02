
using UnityEngine;
using System.Collections.Generic;

public class CoinController : NPCController
{
    [SerializeField] CoinData _data;

    public CoinData Data{get=>_data;}
    public int Points{get=>_data.Points;}

    public CoinStateID CurrentCoinStateID{get => (CoinStateID) CurrentStateID;}
    public bool IsCollect{get => CurrentCoinStateID == CoinStateID.COLLECT;}

    public bool CanBeCollected{get=> !IsCollect;}

    struct AnimationHash
    {
        public static readonly int Move = AtH("Move");
        public static readonly int Collect = AtH("Collect");

        static int AtH(string s) => Animator.StringToHash(s);
    }

    protected override void InitializeStateMachine()
    {
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