using UnityEngine;
using System;
using System.Collections.Generic;

// Flame spawn controllers are created for each y-level where a flame can theoretically spawn.
// They serve two purposes:
// 1. Communicate a good spawn position to NPCManager when trying to spawn a flame
// 2. Register when a flame is present, to prevent spawning another flame on the given y-level
public class FlameSpawnController : MonoBehaviour 
{
    [NonSerialized] public GameObject ActiveFlame;
    public float XOffset;

    public float YLevel{get => transform.position.y;}

    Vector3 _spawnLeft;
    Vector3 _spawnRight;
    List<Vector3> _spawns;

    void Awake() 
    {
        _spawnLeft = new Vector3(transform.position.x - XOffset, transform.position.y, 0f);
        _spawnRight = new Vector3(transform.position.x + XOffset, transform.position.y, 0f);
    }

    public Vector3 GetSpawn(PlayerController player)
    {   
        return Mathf.Abs(_spawnLeft.x - player.Pos.x) > Mathf.Abs(_spawnRight.x - player.Pos.x)?_spawnLeft:_spawnRight;
    }

    void OnDrawGizmosSelected() 
    {   
        Gizmos.DrawLine(transform.position, transform.position - new Vector3(XOffset, 0, 0));
        Gizmos.DrawLine(transform.position, transform.position + new Vector3(XOffset, 0, 0));
        Gizmos.DrawSphere(transform.position - new Vector3(XOffset, 0, 0),0.25f);
        Gizmos.DrawSphere(transform.position + new Vector3(XOffset, 0, 0),0.25f);
    }
}