public abstract class NPCController : EntityController
{   
    public bool IsTouchingGround{get=>_collisions[CollisionID.GROUND];}
    public bool IsTouchingEntityFront{get=>(IsFacingRight && _collisions[CollisionID.ENTITY_RIGHT]) || (!IsFacingRight && _collisions[CollisionID.ENTITY_LEFT]);}
    public bool IsTouchingSpawnOrExit{get => _collisions[CollisionID.SPAWN_OR_EXIT];}

    public abstract void Delete();
}