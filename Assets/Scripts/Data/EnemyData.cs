using UnityEngine;

[CreateAssetMenu(fileName="EnemyData", menuName="Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public readonly string Name;
    public readonly int PointsOnFlip = 10;
    public readonly int PointsOnKill = 800;
    public readonly float MoveSpeed;
    public readonly float FlippedDuration;
    public readonly Vector2 FlipVector;
    public readonly int MaxAnger = 3;
}