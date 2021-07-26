using UnityEngine;
using System.Collections.Generic;

// All NPCSpawnController does is to report to NPCManager whether it is currently occupied by a NPC.
public class NPCSpawnController : MonoBehaviour 
{   
    [SerializeField] CollisionController _collisionController;

    Dictionary<CollisionID,bool> _collisions = new Dictionary<CollisionID,bool>();

    // The initialized check is required as NPC-Manager might start spawning before collisions are fully initialized, which might result in a KeyNotFoundError
    public bool IsOccupied{get => !_collisionController.Initialized || _collisions[CollisionID.ENTITY_ABOVE];}

    void Update() 
    {   
        if(!_collisionController.Initialized)
            return;

        _collisions = _collisionController.UpdateCollisions();
    }
}