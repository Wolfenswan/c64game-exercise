using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : EntityController
{   
    // Static events subscribed to by classes unaware of the individual player objects (e.g. Enemy- & CoinController)
    public static event Action<EnemyController, PlayerController> TryNPCFlipEvent; // Let subscribers know this enemy has been flipped
    public static event Action<EnemyController, PlayerController> EnemyKillEvent; // Let subscribers know this enemy has been killed
    public static event Action<CoinController, PlayerController> TryCoinCollectEvent; // Let subscribers know this coin has been collected
    public static event Action<PlayerController> PowTriggeredEvent; // Let subscribers know that POW has been triggered and by whom

    // Non-static events subscribed to by objects aware of the individual player objects (PlayerStates and PlayerManager)
    public event Action JumpEvent;
    public event Action<PlayerController, Vector2> HopOtherPlayerEvent; //Sends hit player and contact point
    public event Action<PlayerController> PlayerRespawnEvent;
    public event Action<PlayerController> PlayerDeadEvent;
    public event Action<Vector2, PlayerController> MoveOtherPlayerEvent;
    public event Action<PlayerController> PlayerDisabledEvent;
    
    [SerializeField] PlayerData _data;
    
    PlayerInput _playerInput;
    
    BoxCollider2D _mainCollider;


    #region public field accessors
    public PlayerID ID {get => PlayerValues.ID;}
    public int IDInt {get => PlayerValues.IDInt;}
    public PlayerData Data{get => _data;}
    public PlayerStateID CurrentPlayerStateID{get => (PlayerStateID) CurrentStateID;}
    public PlayerValues PlayerValues{get; private set;}
    public int MovementInput{get;private set;}
    #endregion

    #region state check shorthands
    public bool IsIdle{get => CurrentPlayerStateID == PlayerStateID.IDLE;}
    public bool IsMove{get => CurrentPlayerStateID == PlayerStateID.MOVE;}
    public bool IsSlide{get => CurrentPlayerStateID == PlayerStateID.SLIDE;}
    public bool IsJump{get => CurrentPlayerStateID == PlayerStateID.JUMP;}
    public bool IsSpawn{get => CurrentPlayerStateID == PlayerStateID.SPAWN;}
    public bool IsDie{get => CurrentPlayerStateID == PlayerStateID.DIE;}
    public bool CanHop{get => IsIdle || IsMove || IsSlide;}
    #endregion

    #region collision detection shorthands
    public bool IsTouchingGround{get => _collisions[CollisionID.GROUND];}
    public bool IsTouchingCeiling{get => _collisions[CollisionID.CEILING];}
    public bool IsTouchingEntityAbove{get => _collisions[CollisionID.ENTITY_ABOVE];}
    //public bool IsTouchingOtherPlayerAbove{get => _collisions[CollisionID.PLAYER_ABOVE];}
    public bool IsTouchingOtherPlayerFront{get => (Facing == EntityFacing.RIGHT && _collisions[CollisionID.PLAYER_RIGHT]) || (Facing == EntityFacing.LEFT && _collisions[CollisionID.PLAYER_LEFT]);}
    public bool IsTouchingOtherPlayerDown{get => (_collisions[CollisionID.PLAYER_DOWN]);}
    #endregion

    #region helpers & cooldowns
    int _pointMultiplier;
    float _lastPointAdditionTimestamp = 0; // stores the last time "multipliable" points were scored and is then compared to the current time on the next point collection
    bool _controlsEnabled = true;
    bool _powOnCooldown = false;
    #endregion

    struct AnimationHash
    {
        public static readonly int Idle = AtH("Idle");
        public static readonly int Move = AtH("Move");
        public static readonly int Jump = AtH("Jump");
        public static readonly int Slide = AtH("Slide");
        public static readonly int Die = AtH("Die");

        static int AtH(string s) => Animator.StringToHash(s);
    }

    protected override void Awake() 
    {   
        base.Awake();

        _playerInput = GetComponent<PlayerInput>();
        _mainCollider = GetComponent<BoxCollider2D>();

        _gfxController.SetSpriteColor(_data.AvailableColors[IDInt]);

        StartCoroutine(WaitForCollisionController()); // Disables controls until the collisionController is fully functional
    }

    public void InitializePlayer(PlayerID id, PlayerValues playerValues = null)
    {
        PlayerValues = (playerValues==null)?new PlayerValues(id, _data.StartingLives, 0, _data.BonusLiveTreshold):playerValues;
    }

    protected override void InitializeStateMachine()
    {
        var states = new List<State>
        {
            new PlayerIdleState(PlayerStateID.IDLE, this, AnimationHash.Idle),
            new PlayerSpawnState(PlayerStateID.SPAWN, this, AnimationHash.Idle),
            new PlayerMoveState(PlayerStateID.MOVE, this, AnimationHash.Move),
            new PlayerJumpState(PlayerStateID.JUMP, this, AnimationHash.Jump),
            new PlayerHopState(PlayerStateID.HOP, this, AnimationHash.Jump),
            new PlayerSlideState(PlayerStateID.SLIDE, this, AnimationHash.Slide),
            new PlayerDieState(PlayerStateID.DIE, this, AnimationHash.Die),
        };
        _stateMachine = new StateMachine(states, PlayerStateID.SPAWN, Debugging);
    }

    void OnDisable() 
    {
        PlayerDisabledEvent?.Invoke(this); // Lets PlayerManager know to unsubscribe from all PlayerController events
    }

    void FixedUpdate() 
    {
        ResolveRaycasterCollisions(); 
    }

    #region movement and positioning
    public void SpawnAt(Transform spawn) // Called by PlayerManager
    {
        transform.position = (Vector2) spawn.position;
        UpdateFacing((int) spawn.localScale.x);
        StartCoroutine(SpawnFreeze());
    }

    // input updates are triggered by actions sent through PlayerInput component
    public void OnMove(InputAction.CallbackContext ctx) 
    {   
        MovementInput = _controlsEnabled?Mathf.Clamp((int) ctx.action.ReadValue<float>(),-1,1):0;
    }

    public void OnJump(InputAction.CallbackContext ctx)
    {     
        if (_controlsEnabled && ctx.performed) JumpEvent?.Invoke();
    } 
    #endregion

    #region collisions
    // The trigger is the default hitbox that deals with touching other entities, namely enemies & coins
    void OnTriggerEnter2D(Collider2D other) 
    { 
        if (IsDie) return;

        if (other.gameObject.TryGetComponent<EnemyController>(out EnemyController enemy) && enemy.CanKillPlayer)
        {          
            if (Debugging)
                Debug.Log($"{this} touching {enemy}.");

            if (enemy.IsFlipped)
                EnemyKillEvent?.Invoke(enemy, this);
            else
                Die();
        }
        
        if (other.gameObject.TryGetComponent<CoinController>(out CoinController coin) && coin.CanBeCollected)
        {
            if (Debugging)
                Debug.Log($"{this} touching {coin}.");

            TryCoinCollectEvent?.Invoke(coin, this);
        }

        if (other.gameObject.TryGetComponent<FlameController>(out FlameController flame) && flame.DoesHurt)
        {
            if (Debugging)
                Debug.Log($"{this} touching {flame}.");
                  
            Die();
        }
    }
    
    void ResolveRaycasterCollisions()
    {
        if (IsDie) return;

        var canBounceEntityAbove = IsJump && IsTouchingCeiling && _collisions[CollisionID.ENTITY_ABOVE];
        var canPOW = !_powOnCooldown && IsJump && _collisions[CollisionID.POW];
        //var canBouncePlayerAbove = IsJump && IsTouchingOtherPlayerAbove;
        var canBounceOffPlayerFront = !IsIdle && !IsDie && IsTouchingOtherPlayerFront;
        var canBounceOffPlayerBelow =!IsTouchingGround && IsTouchingOtherPlayerDown;

        if(canBounceEntityAbove)
        {
            var entList = _collisionController.GetLastCollisionHits(CollisionID.ENTITY_ABOVE).Distinct();
            foreach (var ent in entList)
            {
                if (ent.TryGetComponent<CoinController>(out CoinController coin))
                {
                    if (Debugging) Debug.Log($"{this} tries to collect {coin}");
                    TryCoinCollectEvent?.Invoke(coin, this);
                } else if(ent.TryGetComponent<EnemyController>(out EnemyController enemy))
                {   
                    if (Debugging) Debug.Log($"{this} tries to flip {enemy}");
                    TryNPCFlipEvent?.Invoke(enemy, this);
                } else if(ent.TryGetComponent<PlayerController>(out PlayerController otherPlayer))
                {
                    if (Debugging) Debug.Log($"{this} tries to bounce {otherPlayer}"); 

                    // Hitting another player from below can cause that player to execute a smaller jump (hop)
                    if (otherPlayer.CanHop) HopOtherPlayerEvent?.Invoke(otherPlayer, Pos);
                }               
            }
        }

        if (canBounceOffPlayerBelow && canBounceOffPlayerFront)
            Debug.LogWarning($"[PlayerController #{ID}. Conflicting collisions: can bounce both front and down. Current State: {CurrentStateID}. Grounded: {IsTouchingGround}");

        if (canBounceOffPlayerBelow) _stateMachine.ForceState(PlayerStateID.JUMP);
        
        if (canBounceOffPlayerFront)
        {   
            var type = Facing == EntityFacing.RIGHT?CollisionID.PLAYER_RIGHT:CollisionID.PLAYER_LEFT;
            var otherPC = _collisionController.GetLastCollisionHits(type).First().GetComponent<PlayerController>(); // Assumes there can only ever be one distinct player in front

            if (!otherPC.IsDie) ResolveFrontalPlayerBounce(otherPC);
        }

        if (canPOW)
        {
            if (Debugging) Debug.Log($"POW activated by {this}");
            PowTriggeredEvent?.Invoke(this);
            StartCoroutine(PowCooldown());
        }
    }

    void ResolveFrontalPlayerBounce(PlayerController otherPC)
    {   
        if (Debugging)
            Debug.Log($"Player #{ID}({CurrentStateID}), facing {Facing} bounced off player #{otherPC.ID}({otherPC.CurrentStateID}), facing {otherPC.Facing}.");
        
        if (IsMove || IsSlide)
        {   
            if (otherPC.IsIdle)
            {   
                var pushVector = new Vector2(_data.MoveSpeed * (int) Facing, 0f);
                MoveOtherPlayerEvent?.Invoke(pushVector, otherPC);
            } else if (otherPC.IsMove || otherPC.IsSlide || otherPC.IsJump)
            {
                ReverseFacing(); // No need to send event, as other PlayerContoller mirrors this behavior

                if (!IsSlide)
                    _stateMachine.ForceState(PlayerStateID.SLIDE);

                if (otherPC.Facing == Facing)
                    Debug.LogWarning($"[PlayerController #{ID}] Unexpected event: {this} bounced {otherPC} but they are moving the same direction.");
            } else
            {
                Debug.LogError($"[PlayerController #{ID}] Trying to bounce other player #{otherPC.ID} in unexpected state {otherPC.CurrentStateID}. Own state: {CurrentStateID}.");
            }
        } else if (IsJump)
        {
            ((PlayerJumpState) _stateMachine.CurrentState).ReverseJumpDirection();
        } else 
        {
            Debug.LogError($"[PlayerController #{ID}] Trying to bounce other player, from own unexpected state: {CurrentStateID}. Other state: {otherPC.CurrentStateID}");
        }
    }
    #endregion

    #region interaction with other entities
    void Die()
    {
        _stateMachine.ForceState(PlayerStateID.DIE);
        PlayerValues.AddOrRemoveLive(-1);
        StartCoroutine(RespawnTimer());       
    }

    public void ForceToHop()
    {
        _stateMachine.ForceState(PlayerStateID.HOP);
    }

    public void AddPoints(int points, PointTypeID type)
    {   
        if (_data.PointTypesMultiplied[type])
        {
            if (Time.time - _lastPointAdditionTimestamp < _data.PointMultiplierResetAfter)
                _pointMultiplier = Mathf.Min(_pointMultiplier + 1, _data.MaxPointMultiplier);
            else
                _pointMultiplier = 1;

            points*=_pointMultiplier;

            _lastPointAdditionTimestamp = Time.time;
        }
        PlayerValues.UpdateScore(points);
    }
    #endregion

    #region co-routines
    // IEnumerator CollectionMultiplierIncrease() //* OBSOLETE
    // {   
    //     if (Debugging) Debug.Log($"{this} | Collection multiplier CR started. Currently at x{_collectionMultiplier}");
    //     _collectionMultiplier = Mathf.Clamp(_collectionMultiplier + 1, 1, 5);
    //     yield return new WaitForSeconds(_data.CollectionMultiplierResetAfter);
    //     if (Debugging) Debug.Log($"{this} | Collection multiplier CR ended");
    //     _collectionMultiplier = 1;
    // }

    // In adherence to the original arcade game, pow cooldown is individual for each PlayerController so one player can only trigger POW once during their jump but multiple players could - in theory - trigger POW at the same time.
    IEnumerator PowCooldown()
    {   
        _powOnCooldown = true;
        yield return new WaitUntil(() => !IsJump);
        _powOnCooldown = false;
    }

    IEnumerator RespawnTimer()
    {   
        //* Consider alternative: use a WaitUntil for player to be out of camera or combine them; so it's waitUntil, then waitForSeconds
        if (Debugging)
            Debug.Log($"{this} dies and has now {PlayerValues.Lives} lives left.");
        yield return new WaitForSeconds(_data.RespawnDelay);
        if (PlayerValues.Lives > 0)
            PlayerRespawnEvent.Invoke(this);
        else
            PlayerDeadEvent.Invoke(this);
    }

    // The SpawnFreeze CoRoutine acts as buffer between player (re)spawning and retaking controls. Mainly it prevents button presses being read as input when it is not intended.
    IEnumerator SpawnFreeze()
    {   
        var tick = Time.time;
        StartCoroutine(InputBuffer());
        yield return new WaitUntil(() => _controlsEnabled && _mainCollider != null && _stateMachine != null); // The null checks are safety measures to make sure Player Object has been fully initialized
        if (Debugging) Debug.Log($"{this} player controls free.");
        _stateMachine.ForceState(PlayerStateID.SPAWN);
        //_mainCollider.enabled = false;
        yield return new WaitUntil(() => !IsSpawn);
        if (Debugging) Debug.Log($"{this} unlocked.");
        //_mainCollider.enabled = true;
    }

    // A button press used to join the game should not directly translate into movement
    // thus this coroutine prevents registering movement input for a frame
    IEnumerator InputBuffer()
    {   
        _controlsEnabled = false;
        yield return new WaitForEndOfFrame();
        _controlsEnabled = true;
    }
    
    // Due to Unity's initialization structure, the InputSystem could send commands to PlayerController before the CollisionController sub-component is fully initialized.
    // This coroutine makes sure that controls are only accepted after the CollisionController has finished its setup.
    IEnumerator WaitForCollisionController()
    {   
        _controlsEnabled = false;
        yield return new WaitUntil(() => _collisionController.Initialized);
        _controlsEnabled = true;
    }
    #endregion
}