using UnityEngine;
using System.Collections.Generic;

// TODO (optional)
// expand the position by using the entity sprite-width / 2 instead of using exit-transforms
// Alternative: set a int-range for the offset and use gizmos to draw them

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))] // collider needs to be a trigger, rigidbody a kinematic body
public class BorderTeleportController : MonoBehaviour 
{
    [SerializeField] Transform _exit;
    [TagSelector][SerializeField] List<string> _tagsToTeleport;

    void OnTriggerEnter2D(Collider2D other) 
    {   
        if (_tagsToTeleport.Contains(other.gameObject.tag))
        {
            var targetPos = new Vector2(_exit.position.x, other.transform.position.y);
            other.transform.position = targetPos;
        }    
    }

}