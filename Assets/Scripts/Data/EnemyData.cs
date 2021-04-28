using UnityEngine;

[CreateAssetMenu(fileName="EnemyData", menuName="Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public float MoveSpeed;
    public float FlippedDuration;
    public Vector2 FlipVector;
}