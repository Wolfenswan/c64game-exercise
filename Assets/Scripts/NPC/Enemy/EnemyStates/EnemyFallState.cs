using UnityEngine;
using System;

public class EnemyFallState : EnemyState 
{
    public EnemyFallState(EnemyStateID id, EnemyController entity, int animationHash) : base(id, entity, animationHash) {}

    Vector3 _moveVector = new Vector3(0f,0f,0f);

    public override Enum Tick()
    {   

        var direction = _entity.Facing;
        var speed = _entity.Data.MoveSpeed; // TODO split into speed for x & y?

        _moveVector.x = (speed * Time.deltaTime) * direction;
        _moveVector.y = (speed * Time.deltaTime) * -1;
        _entity.transform.position += _moveVector;

        if (_entity.IsTouchingGround)
            return EnemyStateID.MOVE;
        
        return null;
    }
}

// Fall is easy, as enemies always fall in an arc
// Just modify transform as needed