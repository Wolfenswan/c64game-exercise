using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;

public class PlayerController : EntityController
{   
    // Static events subscribed to by classes unaware of the individual player objects (e.g. Enemy- & CoinController)
    public static event Action<EnemyController> NPCFlipEvent; // Let subscribers know this enemy has been flipped
    public static event Action<EnemyController> EnemyKillEvent; // Let subscribers know this enemy has been killed
    public static event Action<CoinController> CoinCollectEvent; // Let subscribers know this coin has been collected

    // Non-static events subscribed to by objects aware of the individual player objects (PlayerStates and PlayerManager)
    public event Action JumpEvent;
    public event Action<PlayerController> PlayerRespawnEvent;
    public event Action<PlayerController> PlayerDeadEvent;
    public event Action<Vector2, PlayerController> MoveOtherPlayerEvent;
    public event Action<PlayerController> PlayerDisabledEvent;
    
    [SerializeField] PlayerData _data;
    [SerializeField] GameObject _collisionDetection;
    public bool Debugging = true;
    
    PlayerInput _playerInput;
    
    CollisionController _collisionController;
    Collider2D _mainCollider;
    TextMeshPro _debugText;
    StateMachine _stateMachine;
    Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();

    #region public field accessors
    public int ID {get => PlayerValues.ID;}
    public PlayerValues PlayerValues{get; private set;}
    public int MovementInput{get;private set;}
    public Vector2 Pos{get=>transform.position;}
    public PlayerData Data{get => _data;}    
    public PlayerStateID CurrentStateID{get => (PlayerStateID) _stateMachine.CurrentState.ID;}
    #endregion

    #region state check shorthands
    public bool IsIdle{get => CurrentStateID == PlayerStateID.IDLE;}
    public bool IsMove{get => CurrentStateID == PlayerStateID.MOVE;}
    public bool IsSlide{get => CurrentStateID == PlayerStateID.SLIDE;}
    public bool IsJump{get => CurrentStateID == PlayerStateID.JUMP;}
    //public bool IsArcedJump{get => IsJump && (PlayerJumpState) _stateMachine.CurrentState.IsArced;}
    public bool IsSpawn{get => CurrentStateID == PlayerStateID.SPAWN;}
    public bool IsDie{get => CurrentStateID == PlayerStateID.DIE;}
    #endregion

    #region collision detection shorthands
    public bool IsTouchingGround{get => _collisions[CollisionType.GROUND];}
    public bool IsTouchingCeiling{get => _collisions[CollisionType.CEILING];}
    public bool IsTouchingOtherPlayerFront{get => (Facing == EntityFacing.RIGHT && _collisions[CollisionType.PLAYER_RIGHT]) || (Facing == EntityFacing.LEFT && _collisions[CollisionType.PLAYER_LEFT]);}
    public bool IsTouchingOtherPlayerDown{get => (_collisions[CollisionType.PLAYER_DOWN]);}
    #endregion

    int _collectionMultiplier = 1; // TODO add functionality. max is 5.
    bool _controlsEnabled = true;
    bool _jumped = false;
    bool _hasBounced = false;


    struct PlayerAnimationHash
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

        var states = new List<State>
        {
            new PlayerIdleState(PlayerStateID.IDLE, this, PlayerAnimationHash.Idle),
            new PlayerSpawnState(PlayerStateID.SPAWN, this, PlayerAnimationHash.Idle),
            new PlayerMoveState(PlayerStateID.MOVE, this, PlayerAnimationHash.Move),
            new PlayerJumpState(PlayerStateID.JUMP, this, PlayerAnimationHash.Jump),
            new PlayerSlideState(PlayerStateID.SLIDE, this, PlayerAnimationHash.Slide),
            new PlayerDieState(PlayerStateID.DIE, this, PlayerAnimationHash.Die),
        };
        _stateMachine = new StateMachine(states, PlayerStateID.SPAWN, Debugging);

        _playerInput = GetComponent<PlayerInput>();
        _gfxController = GetComponent<GFXController>();
        _mainCollider = GetComponent<BoxCollider2D>();
        _collisionController = _collisionDetection.GetComponent<CollisionController>();
        _debugText = transform.Find("DebugText").GetComponent<TextMeshPro>();

        _gfxController.SetSpriteColor(_data.PlayerColors[ID]);

        StartCoroutine(WaitForCollisionController()); // Disables controls until the collisionController is fully functional
    }

    public void InitializePlayer(int id, PlayerValues playerValues = null)
    {
        PlayerValues = (playerValues==null)?new PlayerValues(id, _data.DefaultLives, 0, _data.BonusLiveTreshold):playerValues;
    }

    void OnDisable() 
    {
        PlayerDisabledEvent?.Invoke(this);
    }

    void Update() 
    {   
        if (!_collisionController.Initialized)
            return;
            
        _collisions = _collisionController.UpdateCollisions();

        _stateMachine.Tick(TickMode.UPDATE);    

        ResolveEntityCollisions();  

        if(Debugging)
            _debugText.text = $"Player #{ID}\n{CurrentStateID}";
    }
    void OnTriggerEnter2D(Collider2D other) 
    {   
        // This trigger represents the player's default hitbox which takes care of removing enemies, collecting coins or dying to enemies
        Debug.Log(other.gameObject);

        if (other.gameObject.tag == "Enemy")
        {
            Debug.Log("Touching Enemy!");
            var enemy = other.gameObject.GetComponent<EnemyController>();
            if (enemy.IsFlipped)
                KillEnemy(enemy);
            else
                Die();
        }
        
        if (other.gameObject.tag == "Coin")
        {
            Debug.Log("Touching Coin!");
            CollectCoin(other.gameObject);
        }
    }

    public void ForceState(PlayerStateID newState) => _stateMachine.ForceState(newState);

    #region movement and positioning

    public void ReverseJumpDirection() => ((PlayerJumpState) _stateMachine.CurrentState).ReverseJumpDirection(); // Reversing the jump direction should not modify the facing

    public void PlaceAtSpawn(Transform spawn) // Called by PlayerManager
    {
        transform.position = (Vector2) spawn.position;
        UpdateFacing((int) spawn.localScale.x);
        StartCoroutine(SpawnFreeze());
    }
    #endregion

    #region input update - triggered by actions sent through PlayerInput component
    public void OnMove(InputAction.CallbackContext ctx) 
    {   
        if (Debugging)
            Debug.Log($"Player received move event"); // No better description as refering to any of the PC's fields prompts a NullRef in the event callback

        MovementInput = _controlsEnabled?Mathf.Clamp((int) ctx.action.ReadValue<float>(),-1,1):0;
    }

    //public void OnJump(InputAction.CallbackContext ctx) => JumpInput = _controlsEnabled?ctx.performed:false;
    public void OnJump(InputAction.CallbackContext ctx)
    {   
        if (Debugging)
            Debug.Log($"Player received Jump event"); // No better description as refering to any of the PC's fields prompts a NullRef in the event callback
             
        if (_controlsEnabled && ctx.performed) JumpEvent?.Invoke();
        if (_controlsEnabled) _jumped = ctx.performed; // Only used by the RespawnFreeze
    } 
    #endregion

    #region collisions
    void ResolveEntityCollisions()
    {
        var canFlipEntity = IsJump && IsTouchingCeiling;
        var canFlipEnemy = canFlipEntity && _collisions[CollisionType.ENEMY_ABOVE];
        var canFlipCoin = canFlipEntity && _collisions[CollisionType.COIN_ABOVE];
        var canBounceOffPlayerFront = !IsIdle && !IsDie && IsTouchingOtherPlayerFront;
        var canBounceOffPlayerBelow =!IsTouchingGround && IsTouchingOtherPlayerDown;

        if (canFlipEnemy)
        { // CanFlipEnemy detects if the player is hitting a platform from below while an enemy is standing on it
            _collisionController
                .GetLastCollisionHits(CollisionType.ENEMY_ABOVE).Distinct()
                .Where(enemy => enemy.GetComponent<EnemyController>().IsTouchingGround).ToList()
                .ForEach(enemy => TryFlipEnemy(enemy));
        }

        if (canFlipCoin)
        {
            _collisionController
                .GetLastCollisionHits(CollisionType.COIN_ABOVE).Distinct()
                .Where(coin => coin.GetComponent<CoinController>().CanBeCollected).ToList()
                .ForEach(coin => CollectCoin(coin));
        }

        if (canBounceOffPlayerBelow && canBounceOffPlayerFront)
            Debug.LogWarning($"[PlayerController #{ID}. Conflicting collisions: can bounce both front and down. Current State: {CurrentStateID}. Grounded: {IsTouchingGround}");

        if (canBounceOffPlayerBelow)
            _stateMachine.ForceState(PlayerStateID.JUMP);
        
        if (canBounceOffPlayerFront)
        {   
            var type = Facing == EntityFacing.RIGHT?CollisionType.PLAYER_RIGHT:CollisionType.PLAYER_LEFT;
            var otherPC = _collisionController.GetLastCollisionHits(type).First().GetComponent<PlayerController>(); // Assumes there can only ever be one distinct player in front

            if (!otherPC.IsDie)
                ResolveFrontalBounce(otherPC);
        }
    }
    void ResolveFrontalBounce(PlayerController otherPC)
    {   
        if (Debugging)
            Debug.Log($"Player #{ID}({CurrentStateID}), facing {Facing} bounced off player #{otherPC.ID}({otherPC.CurrentStateID}), facing {otherPC.Facing}.");
        
        if (IsMove || IsSlide)
        {   
            if (otherPC.IsIdle)
            {   
                var pushVector = new Vector2(_data.MoveSpeed * Time.deltaTime * (int) Facing, 0f);
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

    void CollectCoin(GameObject coin)
    {
        CoinCollectEvent?.Invoke(coin.GetComponent<CoinController>());
        AddPoints(_data.CoinCollectPoints);
    }

    void TryFlipEnemy(GameObject enemy)
    {   
        var ec = enemy.GetComponent<EnemyController>();
        NPCFlipEvent?.Invoke(ec);
        if(ec.GivePointsOnFlip) // TODO might not be the most robust method; experiment with receiving answering events from ec if point gain is odd
            AddPoints(ec.FlipPoints);
    }

    void KillEnemy(EnemyController enemy)
    {
        EnemyKillEvent?.Invoke(enemy);
        AddPoints(enemy.KillPoints * _collectionMultiplier);
        // TODO start or reset collectionTimer coRoutine
    }

    void Die()
    {
        _stateMachine.ForceState(PlayerStateID.DIE);
        PlayerValues.AddOrRemoveLive(-1);
        _mainCollider.enabled = false;
        StartCoroutine(RespawnTimer());       
    }
    #endregion

    void AddPoints(int points) => PlayerValues.UpdateScore(points);

    IEnumerator CollectionCountdown()
    {   
        // Start a short countdown to reset the selection multiplier afterwards
        return null;
    }

    IEnumerator RespawnTimer()
    {   
        // Wait for a few seconds so the animation has played
        //* Consider alternative: use a WaitUntil for player sprite to be out of camera or combine them; so it's waituntil -> wait 2 seconds
        yield return new WaitForSeconds(_data.RespawnTime);
        if (PlayerValues.Lives > 0)
            PlayerRespawnEvent.Invoke(this);
        else
            PlayerDeadEvent.Invoke(this);
    }

    IEnumerator SpawnFreeze()
    {   
        var tick = Time.time;
        StartCoroutine(InputBuffer());
        yield return new WaitUntil(() => _controlsEnabled && _mainCollider != null && _stateMachine != null);
        _stateMachine.ForceState(PlayerStateID.SPAWN);
        _mainCollider.enabled = false;
        Debug.Log("Controls free");
        yield return new WaitUntil(() => (MovementInput != 0 || _jumped || Time.time - tick > 2f));
        Debug.Log("Player unlocked");
        _mainCollider.enabled = true; // Disabled by the death-state
    }

    IEnumerator InputBuffer()
    {   
        // Prevents registering movement input for a frame, e.g. when spawning players
        _controlsEnabled = false;
        yield return new WaitForEndOfFrame();
        _controlsEnabled = true;
    }
    
    IEnumerator WaitForCollisionController()
    {   
         // Prevents registering movement input for a frame, e.g. when spawning players
        _controlsEnabled = false;
        yield return new WaitUntil(() => _collisionController.Initialized);
        _controlsEnabled = true;
    }
}