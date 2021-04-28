using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour 
{   
    // Static events subscribed to by classes unaware of the individual player objects (e.g. Enemy- & CoinController)
    public static event Action<EnemyController> EnemyFlipEvent; // Let subscribers know this enemy has been flipped
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
    
    PlayerControls _playerControls;
    PlayerInput _playerInput;
    GFXController _gfxController;
    CollisionController _collisionController;
    Collider2D _mainCollider;
    StateMachine _stateMachine;
    Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();

    #region public field accessors
    public int Facing{get; private set;} = 1;
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
    public bool IsSpawn{get => CurrentStateID == PlayerStateID.SPAWN;}
    public bool IsDie{get => CurrentStateID == PlayerStateID.DIE;}
    #endregion

    #region collision detection shorthands
    public bool IsTouchingGround{get => _collisions[CollisionType.GROUND];}
    public bool IsTouchingCeiling{get => _collisions[CollisionType.CEILING];}
    public bool IsTouchingOtherPlayerFront{get => (Facing == 1 && _collisions[CollisionType.PLAYER_RIGHT]) || (Facing == -1 && _collisions[CollisionType.PLAYER_LEFT]);}
    public bool CanFlipEntity{get=> IsJump && IsTouchingCeiling;}
    public bool CanFlipEnemy{get => CanFlipEntity && _collisions[CollisionType.ENEMY_ABOVE];}
    public bool CanFlipCoin{get => CanFlipEntity && _collisions[CollisionType.COIN_ABOVE];}
    public bool CanBouncePlayer{get => (!IsIdle && !IsDie && IsTouchingOtherPlayerFront);}
    #endregion

    int _collectionMultiplier = 1;
    bool _controlsEnabled = true;
    bool _jumped = false;
    Vector3 _moveVector = new Vector3(0f,0f,0f);

    struct PlayerAnimationHash
    {
        public static readonly int Idle = AtH("Idle");
        public static readonly int Move = AtH("Move");
        public static readonly int Jump = AtH("Jump");
        public static readonly int Fall = AtH("Fall");
        public static readonly int Slide = AtH("Slide");
        public static readonly int Die = AtH("Die");

        static int AtH(string s) => Animator.StringToHash(s);
    }

    void Awake() 
    {   
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

        _playerControls = new PlayerControls();
        _playerInput = GetComponent<PlayerInput>();
        _gfxController = GetComponent<GFXController>();
        _mainCollider = GetComponent<BoxCollider2D>();
        _collisionController = _collisionDetection.GetComponent<CollisionController>();

        StartCoroutine(WaitForCollisionController()); // Disables controls until the collisionController is fully functional
    }

    public void InitializePlayer(int id, PlayerValues playerValues = null)
    {
        PlayerValues = (playerValues==null)?new PlayerValues(id, _data.DefaultLives, 0):playerValues;
    }

    void OnDisable() 
    {
        PlayerDisabledEvent?.Invoke(this);
    }

    void Update() 
    {   
        //UpdateInput();
        //UpdateFacing();
        _collisions = _collisionController.UpdateCollisions();

        if (!_collisionController.Initialized)
            return;
        
        _stateMachine.Tick(TickMode.UPDATE);

        if (CanFlipEnemy) // CanFlipEnemy detects if the player is hitting a platform from below while an enemy is standing on it
            _collisionController.GetLastCollisionHits(CollisionType.ENEMY_ABOVE).Distinct().Where(enemy => enemy.GetComponent<EnemyController>().IsTouchingGround).ToList().ForEach(enemy => TryFlipEnemy(enemy));
            //_collisionController.GetLastCollisionHits(CollisionType.ENEMY_ABOVE).Distinct().Where(enemy => enemy.GetComponent<EnemyController>().CanBeFlipped).ToList().ForEach(enemy => FlipEnemy(enemy));
        
        if (CanFlipCoin)
            _collisionController.GetLastCollisionHits(CollisionType.COIN_ABOVE).Distinct().ToList().ForEach(coin => CollectCoin(coin));

        if (CanBouncePlayer)
        {   
            var type = Facing == 1?CollisionType.PLAYER_RIGHT:CollisionType.PLAYER_LEFT;
            var otherPC = _collisionController.GetLastCollisionHits(type).First().GetComponent<PlayerController>(); // Assumes there can only ever be one distinct player in front

            if (IsMove || IsSlide)
            {
                if (otherPC.IsIdle)
                {   
                    var pushVector = new Vector2(_data.MoveSpeed * Time.deltaTime * Facing, 0f);
                    MoveOtherPlayerEvent?.Invoke(pushVector, otherPC);
                } else if (otherPC.IsMove)
                {
                    Debug.Log("Ground bounce against Move");
                    // TODO
                    // bounce both players off each other
                    // maybe force both to slide in opposing directions?

                    // TODO what happens if the other player is moving away from this player; is it even possible; as they move at the same speed?
                } else if (otherPC.IsSlide)
                {   
                    Debug.Log("Ground bounce against Slide");
                    // TODO check what happens in the original
                } else
                {
                    Debug.LogError($"Trying to bounce other player in unexpected state {otherPC.CurrentStateID}. Own state: {CurrentStateID}.");
                }
            } else if (IsJump)
            {
                if (otherPC.IsJump || otherPC.IsSpawn)
                {
                    Debug.Log("Air bounce");
                    // TODO
                    // bounce both players off each other
                    // maybe flip a bounced bool for both? Does the facing flip in the original?
                    // I think they keep the facing but bounce in an upward arc away from each other
                    // Maybe easiest with a dedicated "AirBounce" state?
                } else
                {
                    Debug.LogError($"Trying to bounce other player in unexpected state {otherPC.CurrentStateID}. Own state: {CurrentStateID}.");
                }
            } else 
            {
                Debug.LogError($"Trying to bounce other player, with own state unexpected : {CurrentStateID}. Other state: {otherPC.CurrentStateID}");
            }
        }
        
    }

    #region movement and positioning
    public void MoveStep(float x, float y)
    {
        _moveVector.Set(x, y, 0f);
        transform.position += _moveVector;
    }

    public void UpdateFacing(int newFacing)
    {   
        if (Facing != newFacing) Facing = newFacing;
    }

    public void PlaceAtSpawn(Transform spawn)
    {
        transform.position = (Vector2) spawn.position;
        UpdateFacing((int) spawn.localScale.x);
        StartCoroutine(SpawnFreeze());
    }
    #endregion

    #region input update - triggered by actions sent through Player Input component
    public void OnMove(InputAction.CallbackContext ctx) => MovementInput = _controlsEnabled?(int) ctx.action.ReadValue<float>():0;
    //public void OnJump(InputAction.CallbackContext ctx) => JumpInput = _controlsEnabled?ctx.performed:false;
    public void OnJump(InputAction.CallbackContext ctx)
    {   
        if (_controlsEnabled && ctx.performed) JumpEvent?.Invoke();
        if (_controlsEnabled) _jumped = ctx.performed; // Only used by the RespawnFreeze
    } 
    #endregion

    void OnTriggerEnter2D(Collider2D other) 
    {   
        Debug.Log(other.gameObject);
        // This trigger represents the player's default hitbox which takes care of removing enemies, collecting coins or dying to enemies
        // TODO this trigger must not overlap with the raycast checking for enemies through platforms (i.e. it should not be heigher than head)
        //* CONSIDER Maybe instead of trigger, also use raycast?

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

        // TODO bouncing other players?
        // If grounded & other is not moving: push other player
        // If grounded & running towards each other: turn around & slide
        // if jump: bounce back a bit & descend -> send an event to JumpState? Flip a public HasBounced bool?
    }

    // https://www.youtube.com/watch?v=_5pOiYHJgl
    
    void CollectCoin(GameObject coin)
    {
        CoinCollectEvent?.Invoke(coin.GetComponent<CoinController>());
        AddPoints(10); // TODO data ; also multiplier?
    }

    void TryFlipEnemy(GameObject enemy)
    {   
        var ec = enemy.GetComponent<EnemyController>();
        EnemyFlipEvent?.Invoke(ec);
        if(ec.GivePointsOnFlip) // TODO might not be the most robust method; experiment with receiving answering events from ec if point gain is odd
            AddPoints(10); // TODO data
    }

    void KillEnemy(EnemyController enemy)
    {
        EnemyKillEvent?.Invoke(enemy);
        AddPoints(10); // TODO data // enemyCollectPoints * collectionBonus
        // TODO start or reset collectionTimer coRoutine
    }

    void AddPoints(int points) => PlayerValues.UpdateScore(points);

    void Die()
    {
        _stateMachine.ForceState(PlayerStateID.DIE);
        PlayerValues.AddOrRemoveLive(-1);
        _mainCollider.enabled = false;
        StartCoroutine(RespawnTimer());       
    }

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
        yield return new WaitUntil(() => (MovementInput != 0 || _jumped || Time.time - tick > 2f));
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