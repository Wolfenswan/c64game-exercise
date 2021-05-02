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
    protected Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();
    
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
        if(Debugging)
            _debugText.text = $"{this}\n{CurrentStateName}";
    }

    #region movement and facing   
    public void MoveStep(float x, float y) => transform.position += new Vector3(x, y, 0f);

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
    #endregion
}