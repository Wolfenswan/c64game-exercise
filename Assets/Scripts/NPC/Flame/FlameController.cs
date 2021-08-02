using UnityEngine;
using System.Collections.Generic;

public class FlameController : NPCController
{   
    [SerializeField] FlameData _data;

    public FlameData Data{get => _data;}
    public bool DoesHurt{get => ((FlameState) CurrentState).DoesHurt;}

    // Doesnt do much:
    // 1. spawn every x seconds on either side of arena, on height of player (managed by NPCManager)
    //      - spawn time is either random, or fixed but dependent on player presence (i.e. both players have a flame that hunts them)
    //      - exception: if player is currently spawning, the flame spawns one level below and cant hit the player
    // 2. Fly from left to right; bouncing every x seconds; bounce is not random
    // 3. Disappear when touching other end of arena (still visible on screen) or player or POW hit

    struct AnimationHash
    {
        public static readonly int Move = AtH("Move");
        public static readonly int Spawn = AtH("Spawn");
        public static readonly int Despawn = AtH("Despawn"); //! check in original; might be the same animation as spawn

        static int AtH(string s) => Animator.StringToHash(s);
    }

    protected override void InitializeStateMachine()
    {
        var states = new List<State>
        {
            new FlameMoveState(FlameStateID.MOVE, this, AnimationHash.Move, doesHurt:true),
            new FlameBounceState(FlameStateID.BOUNCE, this, AnimationHash.Move, doesHurt:true),
            new FlameSpawnState(FlameStateID.SPAWN, this, AnimationHash.Spawn, doesHurt:false),
            new FlameDespawnState(FlameStateID.DESPAWN, this, AnimationHash.Despawn, doesHurt:false),
        };
        _stateMachine = new StateMachine(states, FlameStateID.SPAWN, true);
    }

    void OnEnable() 
    {
        _gfxController.AnimationFinishedEvent += GFX_AnimationFinishedEvent;
    }

    void OnDisable() 
    {
        _gfxController.AnimationFinishedEvent -= GFX_AnimationFinishedEvent;
    }

    public override void Delete()
    {
        // TODO if not IsDespawn -> forceState Despawn
        throw new System.NotImplementedException();
    }

    void GFX_AnimationFinishedEvent(int animHash)
    {   
        // if animHash == animHash.Spawn -> forceState Move // This lazy; correct way would be to check inside FlameSpawnState. But functionally there's no difference.
        // if animHash == animHash.Despawn -> Destroy object
    }
}