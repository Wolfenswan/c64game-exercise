using UnityEngine;
using System.Collections.Generic;

public class NPCSpawnController : MonoBehaviour 
{   
    [SerializeField] CollisionController _collisionController;

    Dictionary<CollisionType,bool> _collisions = new Dictionary<CollisionType,bool>();

    // The initialized check is required as NPC-Manager might start spawning before collisions are fully initialized, which might result in a KeyNotFoundError
    public bool IsOccupied{get => !_collisionController.Initialized || _collisions[CollisionType.NPC_ABOVE];}

    private void Update() 
    {   
        if(!_collisionController.Initialized)
            return;

        _collisions = _collisionController.UpdateCollisions();
    }
}