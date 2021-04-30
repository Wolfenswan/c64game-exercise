using UnityEngine;
using System.Collections.Generic;

// TODO (optional)
// expand the position by using the entity sprite-width / 2 instead of using exit-transforms
// Alternative: set a int-range for the offset and use gizmos to draw them

[RequireComponent(typeof(BoxCollider2D), typeof(Rigidbody2D))]
public class BorderTeleportController : MonoBehaviour 
{
    //public static event Action<GameObject, Vector2, EntityFacing> TeleportEvent; // The entity to teleport,the target position and facing
    [SerializeField] Transform _exit;
    [SerializeField] List<string> _tagsToTeleport;
    //[SerializeField] EntityFacing FacingOnExit = EntityFacing.RIGHT;

    BoxCollider2D _ownCollider;

    void Awake() 
    {
        _ownCollider = GetComponent<BoxCollider2D>();
        _ownCollider.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other) 
    {   
        if (_tagsToTeleport.Contains(other.gameObject.tag))
        {
            var targetPos = new Vector2(_exit.position.x, other.transform.position.y);
            other.transform.position = targetPos;
        }    
    }

}