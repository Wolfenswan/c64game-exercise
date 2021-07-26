using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class EnemyAngerLevel
{
    public int ID;
    public Color Color;
    public float Speed;
}

[CreateAssetMenu(fileName="EnemyData", menuName="Data/Enemy Data")]
public class EnemyData : ScriptableObject
{
    public string Name;
    public int PointsOnFlip = 10;
    public int PointsOnKill = 800;
    public float FlippedDuration;
    public Vector2 FlipVector;
    public Vector2 DieVector;
    public List<EnemyAngerLevel> AngerLevels;
}