using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof (GFXController))]
public abstract class EntityController : MonoBehaviour, IDebugMonitor
{   
    [Header("EntityController")]
    [SerializeField] GameObject _collisionDetection;

    #region components
    protected GFXController _gfxController;
    protected CollisionController _collisionController;
    #endregion

    #region state handling
    protected StateMachine _stateMachine;
    public State CurrentState{get => _stateMachine.CurrentState;}
    public Enum CurrentStateID{get => _stateMachine.CurrentState.ID;}
    public string CurrentStateName{get => _stateMachine.CurrentState.ToString();}
    #endregion

    public Vector2 Pos{get=>transform.position;}
    public EntityFacing Facing{get; private set;} = EntityFacing.RIGHT;    
    protected Dictionary<CollisionID,bool> _collisions = new Dictionary<CollisionID,bool>();
    
    #region public field accessors
    public bool IsFacingRight{get => Facing == EntityFacing.RIGHT;}
    #endregion

    #region debugging   
    [Header("Debugging")]
    [SerializeField] DebugLogLevel _dbgMaxLogLevel = DebugLogLevel.ALL;
    [SerializeField] TextMeshPro _debugTextField;
    
    public event EventHandler<DebugEventArgs> DebugEvent;
    public event Action<object, string> DebugLogEvent;
    public event Action<object, string> DebugWarningEvent;
    public event Action<object, string> DebugErrorEvent;
    public event Action<object, string> DebugTextUpdateEvent;

    public void DebugRaiseEvent(string debugText, DebugLogType logType = DebugLogType.CONSOLE, DebugLogLevel logLevel = DebugLogLevel.NORMAL, bool overrideLogLevelRestrictions = false)
    {
        var args = new DebugEventArgs(debugText, logType, logLevel);
        DebugEvent?.Invoke(this, args);
    }
    #endregion


    #region initialization
    protected virtual void Awake() 
    {
        _gfxController = GetComponent<GFXController>();
        _collisionController = _collisionDetection.GetComponent<CollisionController>();

        InitializeStateMachine();
    }

    protected virtual void Start()
    {
        DebugManager.Instance.RegisterObject<EntityController>(this, $"{this}", _dbgMaxLogLevel, _debugTextField);
    }

    protected virtual void OnDisable() 
    {
        DebugManager.Instance.DeregisterObject(this);
    }

    protected abstract void InitializeStateMachine();
    #endregion

    protected virtual void Update() 
    {
        if (GameManager.Instance == null || !_collisionController.Initialized)
            return;
        
        _collisions = _collisionController.UpdateCollisions();

        _stateMachine.Tick(TickMode.UPDATE);    

        //DebugRaiseEvent($"{CurrentStateName}.", DebugLogType.TEXT);
        DebugTextUpdateEvent(this, $"{CurrentStateName}");
    }

    #region movement and facing   
    public void MoveStep(float x, float y) => MoveStep(new Vector2(x, y));
    public void MoveStep(float x, float y, bool applyDeltaTime) => MoveStep(new Vector2(x, y) * Time.deltaTime);
    public void MoveStep(Vector2 moveVector) => transform.position += (Vector3) moveVector;
    public void MoveStep(Vector2 moveVector, bool applyDeltaTime) => transform.position += (Vector3) moveVector * Time.deltaTime;

    public void UpdateFacing(int newFacing) =>  UpdateFacing((EntityFacing) newFacing);

    public void UpdateFacing(EntityFacing newFacing)
    {
        if (Facing != newFacing) 
        {
            Facing = newFacing;
            _gfxController?.FlipSprite(x:true);
        }
    }
    public void ReverseFacing() => UpdateFacing(IsFacingRight?EntityFacing.LEFT:EntityFacing.RIGHT);

    public void SetFacingTowards<T> (T otherEntity) where T:EntityController
    {
        var newFacing = otherEntity.Pos.x > Pos.x?EntityFacing.RIGHT:EntityFacing.LEFT;
        UpdateFacing(newFacing);
    }
    #endregion
}