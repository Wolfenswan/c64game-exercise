using System;

public class EnemyTurnState : EnemyState 
{   
    bool _turnFinished;

    public EnemyTurnState(EnemyStateID id, EnemyController entity, int animationHash) : base(id, entity, animationHash) 
    {
        CanBeFlipped = true;
        GivePointsOnFlip = true;
    }

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);

        _turnFinished = true; // TODO debug

        _gfxController.AnimationFinishedEvent += TurnFinished;
    }

    public override Enum Tick()
    {   
        if (_turnFinished)
        {
            _entity.ReverseFacing();
            return EnemyStateID.MOVE;
        }
            
        if (_doFlip)
            return EnemyStateID.FLIPPED;

        return null;
    }

    public override void OnExit(Enum toState)
    {
        base.OnExit(toState);
        _gfxController.AnimationFinishedEvent -= TurnFinished;
    }

    void TurnFinished(int hash) 
    {
        if (hash == _animationHash) _turnFinished = true;
    }
}