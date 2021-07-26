using UnityEngine;

[CreateAssetMenu(fileName="FlameData", menuName="Data/Flame Data")]
public class FlameData : ScriptableObject
{
    public float MoveSpeed = 0.5f;
    public float BounceStrength = 2f;
    public float BounceFrequency = 0.2f;
    public float FirstFlameDelay = 30f;
    public NEINGames.Collections.RangeInt FlameSpawnDelayRange;
}