using UnityEngine;

[CreateAssetMenu(fileName="PlayerData", menuName="Data/Player Data")]
public class PlayerData : ScriptableObject
{
    public float MoveSpeed;
    public float SlideSpeed;
    public float JumpSpeed;
    [Range(0.1f,1f)] public float TimeToSlide;
    [Range(0.1f,1f)] public float SlideDuration;
    [Tooltip("X is max width, Y is max height")] public Vector2 JumpPower;
    public ForceMode2D JumpMode;
    public int DefaultLives = 5;
    public float RespawnTime = 2f;
}