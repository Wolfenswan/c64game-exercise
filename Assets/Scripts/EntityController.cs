using UnityEngine;
using System;
using System.Collections.Generic;
using TMPro;

[RequireComponent(typeof (GFXController))]
public abstract class EntityController : MonoBehaviour 
{   
    public bool Debugging = true;
    [SerializeField] GameObject _collisionDetection;

    #region components
    protected GFXController _gfxController;
    protected CollisionController _collisionController;
    protected TextMeshPro _debugText;
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

    #region initialization
    protected virtual void Awake() 
    {
        _gfxController = GetComponent<GFXController>();
        _collisionController = _collisionDetection.GetComponent<CollisionController>();
        _debugText = transform.Find("DebugText").GetComponent<TextMeshPro>();

        InitializeStateMachine();
    }

    protected abstract void InitializeStateMachine();
    #endregion

    protected virtual void Update() 
    {
        if (GameManager.Instance == null || !_collisionController.Initialized)
            return;
        
        _collisions = _collisionController.UpdateCollisions();

        _stateMachine.Tick(TickMode.UPDATE);    

        if(Debugging)
            _debugText.text = $"{this}\n{CurrentStateName}";

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