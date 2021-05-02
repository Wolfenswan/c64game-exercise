using UnityEngine;
using System.Collections.Generic;
using System;

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))] // collider needs to be a trigger, rigidbody a kinematic body
public class NPCExitController : MonoBehaviour 
{
    public static event Action<GameObject> NPCExitTeleportEvent;

    [TagSelector][SerializeField] List<string> _tagsToTeleport;
    void OnTriggerEnter2D(Collider2D other) 
    {   
        if (_tagsToTeleport.Contains(other.gameObject.tag))
        {
            NPCExitTeleportEvent?.Invoke(other.gameObject);
        }    
    }
}