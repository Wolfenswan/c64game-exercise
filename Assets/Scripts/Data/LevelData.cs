using UnityEngine;
using System.Collections.Generic;
using System;

[Serializable]
public class EnemyPrefabContainer // Custom container to store both the enemy prefab and the # to spawn per level
{   
    // Copy-constructor required by the NPCManager to properly clone the prefab-list from LevelData.
    public EnemyPrefabContainer(EnemyPrefabContainer container)
    {
        Prefab = container.Prefab;
        Count = container.Count;
    }

    public GameObject Prefab;
    public int Count;
}

[CreateAssetMenu(fileName="LevelData", menuName="Data/Level Data")]
public class LevelData : ScriptableObject
{   
    public int LevelID;
    [Tooltip("Prefab and number for each enemy.")] public List<EnemyPrefabContainer> EnemiesToSpawn;
    public bool BonusStage; //* Maybe just set via GameManager each x levels?

    //* Maybe: Platform-color / sprite & background to use
}