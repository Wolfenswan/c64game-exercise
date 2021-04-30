using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName="PlayerData", menuName="Data/Player Data")]
public class PlayerData : ScriptableObject
{
    public float MoveSpeed;
    [Range(0.1f,1f)] public float TimeToSlide;
    [Range(0.1f,1f)] public float SlideDuration;
    public float JumpSpeed;
    public Vector2 JumpVector = new Vector2(2,2);
    public int DefaultLives = 5;
    public int BonusLiveTreshold = 20000;
    public float RespawnTime = 2f;
    public int CoinCollectPoints = 800;
    public List<Color32> PlayerColors;
}