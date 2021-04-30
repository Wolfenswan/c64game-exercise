using System;
using UnityEngine;

public abstract class NPCController : EntityController 
{
    public event Action NPCFlipEvent; // Listened to by the individual states

    public virtual void OnFlip()
    {   
        // Replaced by more advanced implementations in more complex enemies; e.g. flipping to different move state instead of flip on back            
        NPCFlipEvent?.Invoke();
    }
}