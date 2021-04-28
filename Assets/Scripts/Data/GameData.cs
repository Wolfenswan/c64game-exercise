using UnityEngine;

[CreateAssetMenu(fileName="GameData", menuName="Data/Game Data")]
public class GameData : ScriptableObject
{
    public float SpawnDelay = 20f;
    public Vector2 GravityVector = new Vector2(0, -2);
}