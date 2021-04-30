using UnityEngine;

[RequireComponent(typeof (GFXController))]
public abstract class EntityController : MonoBehaviour 
{
    protected GFXController _gfxController;
    
    public EntityFacing Facing{get; private set;} = EntityFacing.RIGHT;
    public bool FacingRight{get => Facing == EntityFacing.RIGHT;}

    protected virtual void Awake() 
    {
        _gfxController = GetComponent<GFXController>();
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
    public void ReverseFacing() => UpdateFacing(FacingRight?EntityFacing.LEFT:EntityFacing.RIGHT);
}