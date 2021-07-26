using System;

public class FlameSpawnState : FlameState 
{
    public FlameSpawnState(FlameStateID id, FlameController entity, int animationHash, bool doesHurt = false) : base(id, entity, animationHash, doesHurt){}

    public override Enum Tick()
    {
        if (_runTime > 2f)
            return FlameStateID.MOVE;

        return null;
    }
}