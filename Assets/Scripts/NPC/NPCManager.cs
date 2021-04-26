using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NPCManager : MonoBehaviour
{
    public event Action AllEnemiesRemovedEvent;

    [SerializeField] GameData _data;
    [SerializeField] GameObject _coinPrefab;

    LevelData _currentLevelData = null;
    List<Transform> _spawnPositions;
    List<EnemyPrefabContainer> _enemiesToSpawn = new List<EnemyPrefabContainer>();
    List<EnemyController> _activeEnemies = new List<EnemyController>();
    List<CoinController> _activeCoins = new List<CoinController>();

    // Use ScriptableObjects to store Data for each level (List of Prefabs etc), pick by Level-ID (set by GameManager)

    void Awake() 
    {
        _spawnPositions = GameObject.FindGameObjectsWithTag("EnemySpawn").Select(g => g.transform).ToList();
    }

    void OnEnable() 
    {
        GameManager.Instance.LevelChangeEvent += GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent += GameManager_LevelStartedEvent;
    }

    void OnDisable() 
    {
        
    }

    IEnumerator SpawnLoop(int levelID) // Starts once the GameManager raises the LevelStartedEvent
    {
        var spawnDelay = _data.SpawnDelay;
        while (levelID == _currentLevelData.LevelID && _enemiesToSpawn.Count > 0)
        {
            yield return new WaitForSeconds(spawnDelay);
            if (levelID == _currentLevelData.LevelID) // Double check to make sure the level hasn't changed within the last spawn delay
            {   
                var idx = UnityEngine.Random.Range(0, _enemiesToSpawn.Count); // TODO probably needs to be count-1
                var enemy = _enemiesToSpawn[idx];

                GameObject newEnemy = GameObject.Instantiate(enemy.Prefab);
                newEnemy.transform.parent = transform;
                newEnemy.name = "${enemy.Prefab} #{enemy.Count}";
                PlaceNPCatSpawn(newEnemy);

                EnemyController ec = newEnemy.GetComponent<EnemyController>();
                _activeEnemies.Add(ec);
                ec.EnemyDeadEvent += EnemyController_EnemyDeadEvent;   

                enemy.Count -= 1;
                if (enemy.Count == 0)
                    _enemiesToSpawn.Remove(_enemiesToSpawn[idx]);
            }     
        }
        
    }

    void PlaceNPCatSpawn(GameObject NPC)
    {   
        var idx = UnityEngine.Random.Range(0, _spawnPositions.Count-1);
        var spawnTr = _spawnPositions[idx];
        NPC.transform.position = (Vector2) spawnTr.position;
        NPC.transform.localScale = spawnTr.localScale;
        NPC.GetComponent<EnemyController>().Facing = (int) spawnTr.localScale.x; // Alternative: use an edgecollider2d in the spawn sprite so they'd turn around. But I kind of like this hacky solution.
    }

    #region event reactions
    void GameManager_LevelChangeEvent(LevelData data) 
    {
        foreach (var item in _activeCoins)
        {
            Destroy(item);
        }
        _currentLevelData = data;
        _enemiesToSpawn.Clear();
        _enemiesToSpawn = _currentLevelData.EnemiesToSpawn;
    }

    void GameManager_LevelStartedEvent()
    {
        StartCoroutine(SpawnLoop(_currentLevelData.LevelID));
    }

    void EnemyController_EnemyDeadEvent(EnemyController ec)
    {
        _activeEnemies.Remove(ec);

        if(_activeEnemies.Count == 0 && _enemiesToSpawn.Count == 0)
            AllEnemiesRemovedEvent?.Invoke();
        else
        {
            GameObject newCoin = GameObject.Instantiate(_coinPrefab);
            PlaceNPCatSpawn(newCoin);
        }
    }
    #endregion
}