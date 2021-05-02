using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NEINGames.Extensions;

// TODO create flames independent from spawn-loop

public class NPCManager : MonoBehaviour
{
    public event Action AllEnemiesRemovedEvent;
    
    [SerializeField] GameData _data;
    [SerializeField] GameObject _coinPrefab;

    LevelData _currentLevelData = null;
    List<EnemyPrefabContainer> _newEnemiesToSpawn = new List<EnemyPrefabContainer>(); // Set on reaction to LevelChangeEvent triggered from the GameManager Singleton
    List<EnemyController> _presentEnemies = new List<EnemyController>();
    List<CoinController> _presentCoins = new List<CoinController>();
    List<FlameController> _presentFlames = new List<FlameController>();

    List<NPCSpawnController> _spawnPositions = new List<NPCSpawnController>();
    List<GameObject> _npcPlacementConveyor = new List<GameObject>(); // Required to prevent coins and enemies from clogging up the spawn points when spawning at the same time

    Transform _enemyGrouping;
    Transform _coinGrouping;
    Transform _flameGrouping;

    #region shorthands
    bool hasActiveEnemies{get=>_presentEnemies.Count > 0;}
    bool isFinishedSpawning{get=>_newEnemiesToSpawn.Count == 0;}
    #endregion

    void Awake() 
    {
        _spawnPositions = transform.Find("Spawns").gameObject.GetChildrenWithTag("NPCSpawn").Select(g => g.GetComponent<NPCSpawnController>()).ToList();

        _enemyGrouping = transform.Find("Enemies");
        _coinGrouping = transform.Find("Coins");
        _flameGrouping = transform.Find("Flames");
    }

    void Start() 
    {
        PlayerController.NPCFlipEvent += PlayerController_EnemyFlipEvent;
        PlayerController.EnemyKillEvent += PlayerController_EnemyKillEvent;
        PlayerController.CoinCollectEvent += PlayerController_CoinCollectEvent;

        NPCExitController.NPCExitTeleportEvent += NPCExitController_NPCExitTeleportEvent;

        GameManager.Instance.LevelChangeEvent += GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent += GameManager_LevelStartedEvent;
    }

    void OnDisable() 
    {
        PlayerController.NPCFlipEvent -= PlayerController_EnemyFlipEvent;
        PlayerController.EnemyKillEvent -= PlayerController_EnemyKillEvent;
        PlayerController.CoinCollectEvent -= PlayerController_CoinCollectEvent;

        NPCExitController.NPCExitTeleportEvent -= NPCExitController_NPCExitTeleportEvent;

        GameManager.Instance.LevelChangeEvent -= GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent -= GameManager_LevelStartedEvent;
    }

    void Update() 
    {   
        // TODO experiment with while loop
        if (_npcPlacementConveyor.Count > 0 && _spawnPositions.Count(p => !p.IsOccupied) > 0)
        {   
            // Activate the first NPC waiting in line at a random free spawn position
            var filtered = _spawnPositions.Where(p => !p.IsOccupied).ToList();
            var spawn = filtered[UnityEngine.Random.Range(0, filtered.Count)].transform;
            SetNPCPosition(_npcPlacementConveyor[0], spawn);
            _npcPlacementConveyor[0].SetActive(true);
            _npcPlacementConveyor.RemoveAt(0);
        }
    }

    IEnumerator EnemySpawnLoop(int levelID) // Starts once the GameManager raises the LevelStartedEvent
    {
        var spawnDelay = _data.SpawnDelay; // TODO move to awake
        Debug.Log(levelID + " // " + _currentLevelData.LevelID + " // " + isFinishedSpawning);
        while (levelID == _currentLevelData.LevelID && !isFinishedSpawning)
        {
            yield return new WaitForSeconds(spawnDelay);
            if (levelID == _currentLevelData.LevelID) // Double check to make sure the level hasn't changed within the last spawn delay
            {   
                var idx = UnityEngine.Random.Range(0, _newEnemiesToSpawn.Count);
                var enemyPrefabData = _newEnemiesToSpawn[idx];
                SpawnNPC(enemyPrefabData.Prefab);
                enemyPrefabData.Count -= 1;
                if (enemyPrefabData.Count == 0)
                    _newEnemiesToSpawn.Remove(_newEnemiesToSpawn[idx]);
            }     
        }     
    }

    void SpawnNPC(GameObject prefab)
    {
        GameObject newNPC = GameObject.Instantiate(prefab);
        newNPC.SetActive(false);

        Debug.Log($"Spawned {newNPC}");

        if (newNPC.GetComponent<EnemyController>())
        {
            newNPC.transform.parent = _enemyGrouping;
            _presentEnemies.Add(newNPC.GetComponent<EnemyController>());
        }
            
        else if (newNPC.GetComponent<CoinController>())
        {
            newNPC.transform.parent = _coinGrouping;
            _presentCoins.Add(newNPC.GetComponent<CoinController>());
        }
        else if (newNPC.GetComponent<FlameController>())
        {   
            newNPC.transform.parent = _flameGrouping;
            _presentFlames.Add(newNPC.GetComponent<FlameController>());
        }
        _npcPlacementConveyor.Add(newNPC);
    }

    void SetNPCPosition(GameObject npc, Transform spawnTr)
    {   
        var idx = UnityEngine.Random.Range(0, _spawnPositions.Count);
        npc.transform.position = (Vector2) spawnTr.position;
        npc.GetComponent<NPCController>().UpdateFacing((int) spawnTr.localScale.x); // Alternative: use an edgecollider2d in the spawn so they'd turn around. But I kind of like this hacky solution.
    }

    void CheckLevelDone()
    {
        if(!hasActiveEnemies && isFinishedSpawning)
        {
            DestroyAllNPC(); // Remove all remaining coins & flames
            AllEnemiesRemovedEvent?.Invoke();
        }
        else
        {
            SpawnNPC(_coinPrefab); // TODO probably as coroutine with delay
            if(_presentEnemies.Count == 1 && isFinishedSpawning)
                _presentEnemies[0].IncreaseAnger(99);
        }
    }

    void DestroyAllNPC()
    {
        foreach (var item in _presentEnemies)
        {
            item.Delete();
        }

        foreach (var item in _presentCoins)
        {
            item.Delete();
        }

        foreach (var item in _presentFlames)
        {
            item.Delete();
        }

        _presentEnemies.Clear();
        _presentCoins.Clear();
        _presentFlames.Clear();
    }

    #region event reactions
    void PlayerController_EnemyFlipEvent(EnemyController enemyHit, PlayerController byPlayer)
    {   
        Debug.Log($"Received flip event for {enemyHit}.");
        enemyHit.OnFlip((Vector2) byPlayer.Pos);
    }

    void PlayerController_EnemyKillEvent(EnemyController enemyHit, PlayerController byPlayer)
    {   
        if (enemyHit.IsDie)
        {
            Debug.LogError($"Received kill event for already dead {enemyHit}.");
            return;
        }

        enemyHit.OnDead((Vector2) byPlayer.Pos);
        _presentEnemies.Remove(enemyHit);

        CheckLevelDone();
    }

    void PlayerController_CoinCollectEvent(CoinController coinCollected)
    {   
        coinCollected.OnCollect();
        _presentCoins.Remove(coinCollected);
    }

    void NPCExitController_NPCExitTeleportEvent(GameObject npc) 
    {
        npc.SetActive(false);
        if(npc.TryGetComponent<CoinController>(out CoinController coin))
        {
            _presentCoins.Remove(coin);
            Destroy(npc);
        } else 
        {
            _npcPlacementConveyor.Add(npc);
        }
    } 

    void GameManager_LevelChangeEvent(LevelData data) 
    {
        if (!isFinishedSpawning)
            Debug.LogError("Level change triggered despite enemies left to spawn");
        
        if (_presentEnemies.Count > 0)
            Debug.LogError("Level change triggered despite active enemies left.");

        _currentLevelData = data;
        _newEnemiesToSpawn.Clear();
        _currentLevelData.EnemiesToSpawn.ForEach(container => _newEnemiesToSpawn.Add(new EnemyPrefabContainer(container))); // Clone the prefab-list to avoid references
    }

    void GameManager_LevelStartedEvent()
    {
        StartCoroutine(EnemySpawnLoop(_currentLevelData.LevelID));
    }
    #endregion
}