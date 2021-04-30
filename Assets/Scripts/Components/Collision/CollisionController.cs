using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum CollisionType 
{
    GROUND,
    CEILING,
    ENTITY_LEFT,
    ENTITY_RIGHT,
    PLAYER_LEFT,
    PLAYER_RIGHT,
    PLAYER_DOWN,
    ENEMY_ABOVE,
    COIN_ABOVE,
}

public class CollisionController : MonoBehaviour {

    [SerializeField] bool _debugLog = false;
    List<RaycasterGroup> _raycasterGroups = new List<RaycasterGroup> {};

    Dictionary<CollisionType,bool> _collisionDict = new Dictionary<CollisionType,bool>(){};

    public GameObject AttachedTo {get => transform.parent.gameObject;}
    public bool Initialized{get; set;} = false;

    void Awake() 
    {       
            foreach(Transform child in transform)
            {
                if (child.GetComponent<RaycasterGroup>() != null && child.gameObject.activeSelf) 
                {   
                    RaycasterGroup raycasterGroup = child.GetComponent<RaycasterGroup>();
                    _collisionDict.Add(raycasterGroup.CollisionType, false);
                    raycasterGroup.Parent = AttachedTo;
                    _raycasterGroups.Add(raycasterGroup);
                }
            }

            Initialized = true;
    }

    public Dictionary<CollisionType,bool> UpdateCollisions() 
    {
        foreach (var rcGroup in _raycasterGroups)
        {   
            bool doesCollide = rcGroup.CheckCollision();
            _collisionDict[rcGroup.CollisionType] = doesCollide;
        }

        if (_debugLog) 
        {
            string debugString = "Current collisions: ";
            foreach(var item in _collisionDict)
            {
                debugString += $"{item.Key}: {item.Value} |";
            }
            Debug.Log(debugString);
        }

        return _collisionDict;
    }

    RaycasterGroup GetRaycasterGroup(CollisionType collisionType) => _raycasterGroups.Single(rcgroup => rcgroup.CollisionType == collisionType);
    List<RaycasterGroup> GetRaycasterGroups(CollisionType collisionType) => _raycasterGroups.FindAll(rcgroup => rcgroup.CollisionType == collisionType);

    public List<Vector2> GetLastCollisionPoints(CollisionType type)
    {
        var raycaster = GetRaycasterGroup(type); // Assumes there's exactly one of the type. Might need adaption in the future.
        List<Vector2> hits = new List<Vector2>();
        foreach (var item in raycaster.LastCollisions)
        {
            hits.Add(item.point);
        }
        return hits;
    }

    public List<GameObject> GetLastCollisionHits(CollisionType type)
    {
        RaycasterGroup raycaster = GetRaycasterGroup(type); // Assumes there's exactly one of the type. Might need adaption in the future.
        List<GameObject> hits = new List<GameObject>();
        foreach (var item in raycaster.LastCollisions)
        {
            hits.Add(item.collider.gameObject);
        }
        return hits;
    }

    // Check whether the Raycaster-Group with the given collisionType(s) touches a specific collider
    public bool IsTouchingSpecific(Collider2D collider, CollisionType collisionType) => GetRaycasterGroup(collisionType).IsTouching(collider);
    public bool IsTouchingSpecific(Collider2D collider, CollisionType[] collisionTypes, bool any = true) 
    {   
        var hits = 0;
        foreach (var ct in collisionTypes)
        {
            if (GetRaycasterGroup(ct).IsTouching(collider))
                {
                    if (any) return true;
                    else hits ++;

                    if (!any && hits == collisionTypes.Length) return true;
                }
        }
        return false;
    }
}