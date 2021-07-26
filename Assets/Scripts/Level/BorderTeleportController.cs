using UnityEngine;
using System.Collections.Generic;
using NEINGames.PropertyDrawers;


[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))] // collider needs to be a trigger, rigidbody a kinematic body
public class BorderTeleportController : MonoBehaviour 
{
    [SerializeField][Range(-50,50)] float _exitPos;
    [TagSelector][SerializeField] List<string> _tagsToTeleport;

    void OnTriggerEnter2D(Collider2D other) 
    {   
        if (_tagsToTeleport.Contains(other.gameObject.tag))
        {   
            var targetPos = new Vector3 (transform.position.x + _exitPos, other.transform.position.y, 0f);
            other.transform.position = targetPos;
        }    
    }

    void OnDrawGizmosSelected() 
    {   
        var targetPos = transform.position + new Vector3(_exitPos, 0, 0);
        Gizmos.DrawLine(transform.position, targetPos);
        Gizmos.DrawSphere(targetPos, 0.25f);
        Gizmos.DrawLine(targetPos, targetPos + new Vector3(0, 10, 0));
        Gizmos.DrawLine(targetPos, targetPos + new Vector3(0, -10, 0));
    }

}