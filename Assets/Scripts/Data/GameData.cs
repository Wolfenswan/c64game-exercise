using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName="GameData", menuName="Data/Game Data")]
public class GameData : ScriptableObject
{
    public float SpawnDelay = 20f;
    public Vector2 GravityVector = new Vector2(0, -2);
    public List<LevelData> Levels;
    public float CameraShakeStrength = 2;
    public float CameraShakeSpeed= 10;
    
    [Header("Debugging")]
    public bool InfinitePow;
}