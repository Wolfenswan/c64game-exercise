using System;

public class FlameDespawnState : FlameState 
{

    public FlameDespawnState(FlameStateID id, FlameController entity, int animationHash, bool doesHurt = false) : base(id, entity, animationHash, doesHurt){}
}