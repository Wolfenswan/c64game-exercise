using System;

public class FlameSpawnState : FlameState 
{
    public FlameSpawnState(FlameStateID id, FlameController entity, int animationHash, bool doesHurt = false) : base(id, entity, animationHash, doesHurt){}
}