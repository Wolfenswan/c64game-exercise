using UnityEngine;

public class FlameController : NPCController
{
    // Doesnt do much:
    // 1. spawn every x seconds on either side of arena, on height of player (managed by NPCManager)
    //      - spawn time is either random, or fixed but dependent on player presence (i.e. both players have a flame that hunts them)
    //      - exception: if player is currently spawning, the flame spawns one level below and cant hit the player
    // 2. Fly from left to right; bouncing every x seconds; bounce is not random
    // 3. Disappear when touching other end of arena (still visible on screen) or player or POW hit
}