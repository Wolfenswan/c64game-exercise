using UnityEngine;

[CreateAssetMenu(fileName="EnemyData", menuName="Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string Name;
    public int PointsOnFlip = 10;
    public int PointsOnKill = 800;
    public float MoveSpeed;
    public float FlippedDuration;
    public Vector2 FlipVector;
    public Vector2 DieVector;
    public int MaxAnger = 3;
}