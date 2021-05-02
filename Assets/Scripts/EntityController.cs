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

    public Vector2 Pos{get=>transform.position;}
    public EntityFacing Facing{get; private set;} = EntityFacing.RIGHT;    
    protected Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();
    
    #region public field accessors
    public abstract string CurrentStateName{get;}
    public bool IsFacingRight{get => Facing == EntityFacing.RIGHT;}
    #endregion

    protected virtual void Awake() 
    {
        _gfxController = GetComponent<GFXController>();
        _collisionController = _collisionDetection.GetComponent<CollisionController>();
        _debugText = transform.Find("DebugText").GetComponent<TextMeshPro>();
    }

    protected virtual void Update() 
    {
        if(Debugging)
            _debugText.text = $"{this}\n{CurrentStateName}";
    }

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
}