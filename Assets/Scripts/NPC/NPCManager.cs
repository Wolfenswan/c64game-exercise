using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

// TODO create flames independent from spawn-loop

public class NPCManager : MonoBehaviour
{
    public event Action AllEnemiesRemovedEvent;

    [SerializeField] GameData _data;
    [SerializeField] GameObject _coinPrefab;

    LevelData _currentLevelData = null;
    List<Transform> _spawnPositions;
    List<EnemyPrefabContainer> _enemiesToSpawn = new List<EnemyPrefabContainer>(); // Set on reaction to LevelChangeEvent triggered from the GameManager Singleton
    List<EnemyController> _activeEnemies = new List<EnemyController>();
    List<CoinController> _activeCoins = new List<CoinController>();

    #region shorthands
    bool hasActiveEnemies{get=>_activeEnemies.Count > 0;}
    bool isFinishedSpawning{get=>_enemiesToSpawn.Count == 0;}
    #endregion

    void Awake() 
    {
        _spawnPositions = GameObject.FindGameObjectsWithTag("EnemySpawn").Select(g => g.transform).ToList();
    }

    void OnEnable() 
    {
        PlayerController.NPCFlipEvent += PlayerController_EnemyFlipEvent;
        PlayerController.EnemyKillEvent += PlayerController_EnemyKillEvent;

        GameManager.Instance.LevelChangeEvent += GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent += GameManager_LevelStartedEvent;
    }

    void OnDisable() 
    {
        PlayerController.NPCFlipEvent -= PlayerController_EnemyFlipEvent;
        PlayerController.EnemyKillEvent -= PlayerController_EnemyKillEvent;

        GameManager.Instance.LevelChangeEvent -= GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent -= GameManager_LevelStartedEvent;
    }

    IEnumerator EnemySpawnLoop(int levelID) // Starts once the GameManager raises the LevelStartedEvent
    {
        var spawnDelay = _data.SpawnDelay;
        while (levelID == _currentLevelData.LevelID && !isFinishedSpawning)
        {
            yield return new WaitForSeconds(spawnDelay);
            if (levelID == _currentLevelData.LevelID) // Double check to make sure the level hasn't changed within the last spawn delay
            {   
                var idx = UnityEngine.Random.Range(0, _enemiesToSpawn.Count); // TODO probably needs to be count-1
                var enemyPrefabData = _enemiesToSpawn[idx];
                SpawnEnemy(enemyPrefabData.Prefab);
                enemyPrefabData.Count -= 1;
                if (enemyPrefabData.Count == 0)
                    _enemiesToSpawn.Remove(_enemiesToSpawn[idx]);
            }     
        }     
    }

    void SpawnEnemy(GameObject prefab)
    {
        GameObject newEnemy = GameObject.Instantiate(prefab);
        newEnemy.transform.parent = transform;
        newEnemy.name = "${enemy.Prefab} #{enemy.Count}";

        _activeEnemies.Add(newEnemy.GetComponent<EnemyController>());
        SetNPCPosition(newEnemy);
    }

    void SpawnCoin()
    {
        GameObject newCoin = GameObject.Instantiate(_coinPrefab);
        _activeCoins.Add(newCoin.GetComponent<CoinController>());
        SetNPCPosition(newCoin);
    }

    void SetNPCPosition(GameObject NPC)
    {   
        // TODO probably a bit too fickle. Needs to check if there's already an enemy present to avoid weird collisions; maybe add a new class with an "occupied" bool? Then use WaitUntil in a coroutine?
        //* Also, how to make sure they walk out of the spawn instead of falling, but the player can't bounce against it? Maybe add a new collision type, spawn-area and check IsTouchingSpawn in Move? Or a new SpawnState, similar to player?
        var idx = UnityEngine.Random.Range(0, _spawnPositions.Count-1);
        var spawnTr = _spawnPositions[idx];
        NPC.transform.position = (Vector2) spawnTr.position;
        NPC.transform.localScale = spawnTr.localScale;
        NPC.GetComponent<EnemyController>().UpdateFacing((int) spawnTr.localScale.x); // Alternative: use an edgecollider2d in the spawn sprite so they'd turn around. But I kind of like this hacky solution.
    }

    #region event reactions
    void PlayerController_EnemyFlipEvent(EnemyController enemyHit)
    {
        _activeEnemies.Find(ec => ec == enemyHit).OnFlip();
    }

    void PlayerController_EnemyKillEvent(EnemyController enemyHit)
    {   
        _activeEnemies.Find(ec => ec == enemyHit).OnDead();
    }

    void PlayerController_CoinCollectEvent(CoinController coinCollected)
    {
        _activeCoins.Find(c => c == coinCollected).OnFlip(); // TODO decide whether to use own method or repurpose onFlip or onDead
    }

    void GameManager_LevelChangeEvent(LevelData data) 
    {
        foreach (var item in _activeCoins)
        {
            Destroy(item);
        }

        if (isFinishedSpawning)
            Debug.LogError("Level change triggered despite enemies left to spawn");
        
        if (_activeEnemies.Count > 0)
            Debug.LogError("Level change triggered despite active enemies left.");

        _currentLevelData = data;
        _enemiesToSpawn.Clear();
        _enemiesToSpawn = _currentLevelData.EnemiesToSpawn;
    }

    void GameManager_LevelStartedEvent()
    {
        StartCoroutine(EnemySpawnLoop(_currentLevelData.LevelID));
    }

    void EnemyController_EnemyDeadEvent(EnemyController ec)
    {
        _activeEnemies.Remove(ec);
        if(!hasActiveEnemies && isFinishedSpawning)
            AllEnemiesRemovedEvent?.Invoke();
        else
        {
            SpawnCoin();
            if(_activeEnemies.Count == 1 && isFinishedSpawning)
                _activeEnemies.First().IncreaseAnger(99);
        }
    }
    #endregion
}