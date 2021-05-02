public abstract class NPCController : EntityController
{   
    public bool IsTouchingGround{get=>_collisions[CollisionType.GROUND];}
    public bool IsTouchingEntityFront{get=>(IsFacingRight && _collisions[CollisionType.ENTITY_RIGHT]) || (!IsFacingRight && _collisions[CollisionType.ENTITY_LEFT]);}
    public bool IsTouchingSpawn{get => _collisions[CollisionType.SPAWN];}

    public abstract void Delete();
}