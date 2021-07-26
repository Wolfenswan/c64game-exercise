
using System;
using UnityEngine;
using NEINGames.Utilities;

public class EnemyDieState : EnemyState 
{

    public EnemyDieState(EnemyStateID id, EnemyController entity, int animationHash) : base(id, entity, animationHash) 
    {
        CanBeFlipped = false;
        GivePointsOnFlip = false;
    }
    Vector2 _velocityVector;

    public override void OnEnter(Enum fromState)
    {
        base.OnEnter(fromState);
        _velocityVector = _entity.Data.DieVector; // TODO move to constructor once final

        var angle = Vector2Utilities.GetDegreesBetweenVectors(_entity.EventContactVector, _entity.Pos);
        Debug.Log(angle);
        //! TODO add facing-adjustment
    }

    public override Enum Tick()
    {   
        _entity.MoveStep(_velocityVector * Time.deltaTime);
        _velocityVector += _gravityVector * Time.deltaTime;

        return null;
    }
}