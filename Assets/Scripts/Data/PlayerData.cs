using UnityEngine;

[CreateAssetMenu(fileName="PlayerData", menuName="Data/Player Data")]
public class PlayerData : ScriptableObject
{
    public float MoveSpeed;
    //public float SlideSpeed;
    [Range(0.1f,1f)] public float TimeToSlide;
    [Range(0.1f,1f)] public float SlideDuration;
    public float JumpSpeed;
    public Vector2 JumpVector = new Vector2(2,2);
    public Vector2 GravityVector = new Vector2(0, -2);
    public int DefaultLives = 5;
    public float RespawnTime = 2f;
}