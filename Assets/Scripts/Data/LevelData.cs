using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class EnemyPrefabContainer // Custom container to store both the enemy prefab and the # to spawn per level
{   
    public GameObject Prefab;
    public int Count;
}

[CreateAssetMenu(fileName="LevelData", menuName="Data/Level Data")]
public class LevelData : ScriptableObject
{   
    public int LevelID;
    //public List<GameObject> EnemyList;
    //[Tooltip("Prefab and number for each enemy.")] public DictionaryOfNumberedEnemyPrefabs EnemiesToSpawn;
    //[Tooltip("Prefab and number for each enemy.")] public DictionaryOfInt Test;
    [Tooltip("Prefab and number for each enemy.")] public List<EnemyPrefabContainer> EnemiesToSpawn;
    public bool BonusStage; // Maybe just set via GameManager?

    // Maybe: Platform-color / sprite to use
}