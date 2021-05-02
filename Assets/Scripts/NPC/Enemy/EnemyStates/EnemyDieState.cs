
using System;
using UnityEngine;

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
        _velocityVector = _entity.Data.DieVector;
    }

    public override Enum Tick()
    {   
        var speed = _entity.Data.MoveSpeed * Time.deltaTime; // TODO add different speed as required
        var newPos = (Vector2) _entity.transform.position + _velocityVector * speed;
        _entity.transform.position = newPos;
        _velocityVector += _gravityVector * Time.deltaTime;

        return null;
    }
}