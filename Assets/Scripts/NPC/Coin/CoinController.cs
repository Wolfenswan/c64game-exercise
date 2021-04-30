
using UnityEngine;

public class CoinController : NPCController
{
    public bool CanBeCollected{get=>true;} // TODO should be false if currently playing the "collected" animation; probably easiest to use states and check against does

    // TODO four movement states;
    // 1. horizontal movement
    // 2. Falling
    // 3. Turning when touching entity
    // 4. Collected state (change anim & rise a bit)
    // MAYBE a spawn state?
}