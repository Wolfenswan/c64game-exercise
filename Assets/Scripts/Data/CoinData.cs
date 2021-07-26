using UnityEngine;

[CreateAssetMenu(fileName="CoinData", menuName="Data/Coin Data")]
public class CoinData : ScriptableObject
{
    public float MoveSpeed;
    public float CollectUpwardsStrength;
    public int Points = 800;
}