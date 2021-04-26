using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour 
{
    public static event Action<PlayerController> PlayerHitEvent;
    public static event Action<PlayerController> PlayerRespawnEvent;
    public static event Action<PlayerController> PlayerDeadEvent;
    public static event Action<PlayerController> ScoreUpdateEvent;
    public static event Action<EnemyController> EnemyFlipEvent;
    public static event Action<EnemyController> EnemyKillEvent;
    public static event Action<CoinController> CoinCollectedEvent;
    public static event Action<int, int> ScoreUpdatedEvent;
    public static event Action<int, int> LivesUpdatedEvent;
    public event Action JumpEvent;
    
    
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
    public bool IsPlayerFrozen{get;private set;}
    //public Rigidbody2D RB2D{get => _rb2d;}
    public PlayerData Data{get => _data;}    
    public PlayerStateID CurrentState{get => (PlayerStateID) _stateMachine.CurrentState.ID;}
    #endregion

    #region collision detection shorthands
    public bool IsTouchingGround{get => _collisions[CollisionType.GROUND];}
    public bool IsTouchingCeiling{get => _collisions[CollisionType.CEILING];}
    public bool CanFlipEntity{get=> CurrentState == PlayerStateID.JUMP && IsTouchingCeiling;}
    public bool CanFlipEnemy{get => CanFlipEntity && _collisions[CollisionType.ENEMY_ABOVE];}
    public bool CanFlipCoin{get => CanFlipEntity && _collisions[CollisionType.COIN_ABOVE];}
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
            new PlayerMoveState(PlayerStateID.MOVE, this, PlayerAnimationHash.Move),
            new PlayerJumpState(PlayerStateID.JUMP, this, PlayerAnimationHash.Jump),
            new PlayerSlideState(PlayerStateID.SLIDE, this, PlayerAnimationHash.Slide),
            new PlayerDieState(PlayerStateID.DIE, this, PlayerAnimationHash.Die),
        };
        _stateMachine = new StateMachine(states, PlayerStateID.IDLE, Debugging);

        _playerControls = new PlayerControls();
        _playerInput = GetComponent<PlayerInput>();
        _gfxController = GetComponent<GFXController>();
        //_rb2d = GetComponent<Rigidbody2D>();
        _mainCollider = GetComponent<Collider2D>();
        _collisionController = _collisionDetection.GetComponent<CollisionController>();
    }

    public void InitializePlayer(int id, PlayerValues playerValues = null)
    {
        PlayerValues = (playerValues==null)?new PlayerValues(id, _data.DefaultLives, 0):playerValues;
        LivesUpdatedEvent?.Invoke(id, PlayerValues.Lives);
        ScoreUpdatedEvent?.Invoke(id, PlayerValues.Score);
    }

    void Update() 
    {   
        //UpdateInput();
        //UpdateFacing();
        _collisions = _collisionController.UpdateCollisions(Facing);

        if (IsPlayerFrozen) // Prevent any state changes or input registration while frozen
            return;
        
        _stateMachine.Tick(TickMode.UPDATE);

        if (CanFlipEnemy) // CanFlipEnemy detects if the player is hitting a platform from below while an enemy is standing on it
            _collisionController.GetLastCollisionHits(CollisionType.ENEMY_ABOVE).Distinct().Where(enemy => enemy.GetComponent<EnemyController>().CanBeFlipped).ToList().ForEach(enemy => FlipEnemy(enemy));
        
        if (CanFlipCoin)
            _collisionController.GetLastCollisionHits(CollisionType.COIN_ABOVE).Distinct().ToList().ForEach(coin => CollectCoin(coin));
        
    }

    #region movement and positioning
    public void MoveStep(float x, float y)
    {
        //var fallSpeed = _player.Data.GravityVector.y * Time.deltaTime; // TODO once the value is final, move to constructor or declare as field
        _moveVector.Set(x, y, 0f);
        transform.position += _moveVector;
    }

    public void UpdateFacing(int newFacing)
    {
        if (Facing != newFacing)
        {
            Facing = newFacing;
            transform.localScale.Set(newFacing,1,1);
        }
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
        _jumped = ctx.performed; // Only used by the RespawnFreeze
    } 
    #endregion

    // TODO rework into trigger
    // Use raycasters for jumping etc. instead of collider
    //void OnCollisionEnter2D(Collision2D other) 
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
            {
                EnemyKillEvent?.Invoke(enemy);
                AddPoints(10); // TODO data // enemyCollectPoints * collectionBonus
                // TODO start or reset collectionTimer coRoutine
            } else
            {
                Die();
            }
        } else if (other.gameObject.tag == "Coin")
        {
            Debug.Log("Touching Coin!");
            CollectCoin(other.gameObject);
        }

        // TODO bouncing other players?
        // If grounded: turn around & slide
        // if jump: bounce back a bit & descend -> send an event to JumpState? Flip a public HasBounced bool?
    }

    // https://www.youtube.com/watch?v=_5pOiYHJgl

    // OnTriggerEnter && other is enemy
    // If other is helpless -> EnemyHitEvent(other) && self getPoints(enemyCollectPoints * collectionBonus) & start/reset timer for collection bonus & collection bonus++
    // If not jumping -> die
    // If jumping and not platform between other and self -> die
    // If jumping and platform between other and self -> EnemyHitEvent(other); self getPoints()

    void CollectCoin(GameObject coin)
    {
        CoinCollectedEvent?.Invoke(coin.GetComponent<CoinController>());
        AddPoints(10); // TODO data ; also multiplier?
    }

    void FlipEnemy(GameObject enemy)
    {
        EnemyFlipEvent?.Invoke(enemy.GetComponent<EnemyController>());
        AddPoints(10); // TODO data ; also multiplier?
    }

    void AddPoints(int points)
    {
        PlayerValues.Score += points;
        ScoreUpdatedEvent.Invoke(PlayerValues.ID, PlayerValues.Score); // CONSIDER maybe only use player id instead of this?
        // Check against 20.000 -> +1 live
        // Smart way to avoid remembering last treshold? maybe just use _milestone and += 20.000 every 1up?
    }

    void Die()
    {
        // TODO ForceState Die
        //_stateMachine.ForceState(PlayerStateID.DIE);
        PlayerValues.Lives -= 1;
        LivesUpdatedEvent.Invoke(PlayerValues.ID, PlayerValues.Lives);
        //_rb2d.bodyType = RigidbodyType2D.Static;
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
        yield return new WaitForSeconds(_data.RespawnTime);
        if (PlayerValues.Lives > 0)
            PlayerRespawnEvent.Invoke(this);
        else
            PlayerDeadEvent.Invoke(this);
    }

    IEnumerator SpawnFreeze()
    {   
        _controlsEnabled = false;
        yield return new WaitForEndOfFrame(); // Prevents registering movement input right after joining
        _controlsEnabled = true;
        var tick = Time.time;
        _stateMachine.ForceState(PlayerStateID.IDLE); // TODO streamline this section to reduce checks
        IsPlayerFrozen = true; // Frozen is checked in Idle-State and prevents any state-change i.e. movement
        yield return new WaitUntil(() => (MovementInput != 0 || _jumped || Time.time - tick > 2f));
        IsPlayerFrozen = false;
        _mainCollider.enabled = true; // Disabled by the death-state
    }
}