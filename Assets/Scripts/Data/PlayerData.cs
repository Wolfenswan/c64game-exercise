using UnityEngine;
using System;
using System.Collections.Generic;

[CreateAssetMenu(fileName="PlayerData", menuName="Data/Player Data")]
public class PlayerData : ScriptableObject
{
    public float MoveSpeed;
    [Range(0.1f,1f)] public float TimeToSlide;
    [Range(0.1f,1f)] public float SlideDuration;
    public Vector2 JumpForceVector = new Vector2(2,2);
    public float HopModifier = 0.75f;
    [Tooltip("Gravity is multiplied by this factor after a jump has reached its peak, accelerating the fall slightly. Only applies to jumps!")] public float FallSpeedMultiplier = 2f;
    public int StartingLives = 5;
    public int BonusLiveTreshold = 20000;
    [Tooltip("How long till the collection multiplier resets to 1.")] public float CollectionMultiplierResetAfter = 1f;
    public float RespawnDelay = 2f;
    [Tooltip("How long the player floats in the air after respawing & not other input occurs")] public float RespawnGracePeriod = 3f;
    [Tooltip("Each player spawned gets the color at the list's index corresponding to his id.")] public List<Color> AvailableColors;

    [NonSerialized] public Dictionary<PointTypeID, bool> PointTypesMultiplied = new Dictionary<PointTypeID, bool> 
    {
        {PointTypeID.FLIP, false},
        {PointTypeID.KILL, true},
        {PointTypeID.COIN, false},
    };
    //public PointsMultiplierDictionary PointTypesAffectByMultipliers;
}

// Initialize custom classes & drawers for the serialized dictionaries
//[Serializable] public class PlayerColorDictionary : SerializableDictionary<int, Color>{};
//[Serializable] public class PointsMultiplierDictionary : SerializableDictionary<PointTypeID, bool>{};
//[CustomPropertyDrawer(typeof(PlayerColorDictionary))] public class PlayerColorDictionaryDrawer : DictionaryDrawer<int, Color> {}
//[CustomPropertyDrawer(typeof(PointsMultiplierDictionary))] public class PointsMultiplierDictionaryDrawer : DictionaryDrawer<PointTypeID, bool> {}