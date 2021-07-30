using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class NPCManager : MonoBehaviour
{   
    public static event Action<int, PlayerController, PointTypeID> GivePointsEvent; // Parameters are amount of points, receiving player and the type-ID of the given point
    public event Action AllEnemiesRemovedEvent;

    [SerializeField] bool _debugging;
    [SerializeField] bool _spawningEnemies;
    [SerializeField] bool _spawningFlames;
    [SerializeField] GameData _gameData;
    [SerializeField] FlameData _flameData;
    [SerializeField] GameObject _coinPrefab;
    [SerializeField] GameObject _flamePrefab;

    LevelData _currentLevelData = null;
    List<EnemyPrefabContainer> _newEnemiesToSpawn = new List<EnemyPrefabContainer>(); // Set on reaction to LevelChangeEvent triggered from the GameManager Singleton
    public List<EnemyController> PresentEnemies{get;private set;} = new List<EnemyController>();
    public List<CoinController> PresentCoins{get;private set;} = new List<CoinController>();
    public List<FlameController> PresentFlames{get;private set;} = new List<FlameController>();

    List<NPCSpawnController> _npcSpawnControllers = new List<NPCSpawnController>();
    List<FlameSpawnController> _flameSpawnControllers = new List<FlameSpawnController>();
    List<GameObject> _npcPlacementConveyor = new List<GameObject>(); // Required to prevent coins and enemies from clogging up the spawn points when spawning at the same time

    // These are only used for sorting purposes in the editor window. The NPC-GameObjects are created as children of their respective sorting.
    Transform _enemyGrouping;
    Transform _coinGrouping;
    Transform _flameGrouping;

    #region shorthands
    bool _hasActiveEnemies{get=>PresentEnemies.Count > 0;}
    bool _isFinishedSpawning{get=>_newEnemiesToSpawn.Count == 0;}
    #endregion

    void Awake() 
    {
        _npcSpawnControllers = gameObject.GetComponentsInChildren<NPCSpawnController>().ToList();
        _flameSpawnControllers = gameObject.GetComponentsInChildren<FlameSpawnController>().ToList();

        _enemyGrouping = transform.Find("Enemies");
        _coinGrouping = transform.Find("Coins");
        _flameGrouping = transform.Find("Flames");
    }

    void Start() 
    {
        PlayerController.TryNPCFlipEvent += PlayerController_TryEnemyFlipEvent;
        PlayerController.EnemyKillEvent += PlayerController_EnemyKillEvent;
        PlayerController.TryCoinCollectEvent += PlayerController_TryCoinCollectEvent;
        PlayerController.PowTriggeredEvent += PlayerController_PowTriggeredEvent;

        NPCExitController.NPCExitTeleportEvent += NPCExitController_NPCExitTeleportEvent;

        PlayerManager.Instance.NewPlayerJoinedEvent += PlayerManager_NewPlayerJoinedEvent;

        GameManager.Instance.LevelChangeEvent += GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent += GameManager_LevelStartedEvent;
    }

    void OnDisable() 
    {
        PlayerController.TryNPCFlipEvent -= PlayerController_TryEnemyFlipEvent;
        PlayerController.EnemyKillEvent -= PlayerController_EnemyKillEvent;
        PlayerController.TryCoinCollectEvent -= PlayerController_TryCoinCollectEvent;
        PlayerController.PowTriggeredEvent -= PlayerController_PowTriggeredEvent;

        NPCExitController.NPCExitTeleportEvent -= NPCExitController_NPCExitTeleportEvent;

        PlayerManager.Instance.NewPlayerJoinedEvent -= PlayerManager_NewPlayerJoinedEvent;

        GameManager.Instance.LevelChangeEvent -= GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent -= GameManager_LevelStartedEvent;
    }

    void Update() 
    {   
        // TODO experiment with while loop
        if (_spawningEnemies && _npcPlacementConveyor.Count > 0 && _npcSpawnControllers.Count(p => !p.IsOccupied) > 0)
        {   
            // Activate the first NPC waiting in line at a random free spawn position
            var filtered = _npcSpawnControllers.Where(p => !p.IsOccupied).ToList();
            var spawnTransform = filtered[UnityEngine.Random.Range(0, filtered.Count)].transform;
            SetNPCPosition(_npcPlacementConveyor[0], spawnTransform);
            _npcPlacementConveyor[0].SetActive(true);
            _npcPlacementConveyor.RemoveAt(0);
        }
    }

    void LateUpdate() 
    {
        // During late update we check if all enemies have been killed, start the cleanup process and raise the event to alert the GameManager that the level is done
        // TODO might be slightly too fast and a very short delay after killing the final enemy desireable gameplay-wise. Will require testing
        // TODO will later require a special check for bonus stages
        if(!_hasActiveEnemies && _isFinishedSpawning)
        {   
            StopAllCoroutines();
            DestroyAllNPC();
            AllEnemiesRemovedEvent?.Invoke();
        }
    }

    // The enemy spawn loop starts once the GameManager raises the LevelStartedEvent
    // and instantiates as well as places all enemy-prefabs provided via LevelData with random delays in between
    IEnumerator EnemySpawnLoop(int levelID)
    {
        var spawnDelay = _gameData.SpawnDelay; //* CONSIDER - randomize?
        while (!_isFinishedSpawning)
        {
            yield return new WaitForSeconds(spawnDelay);
            var idx = UnityEngine.Random.Range(0, _newEnemiesToSpawn.Count);
            var enemyPrefabData = _newEnemiesToSpawn[idx];
            InstantiateNPC(enemyPrefabData.Prefab);
            enemyPrefabData.Count -= 1;
            if (enemyPrefabData.Count == 0)
                _newEnemiesToSpawn.Remove(_newEnemiesToSpawn[idx]);
        }     
    }

    // Flames are created independently from the enemy spawn loop and are tied to each existing player
    // They dont start spawning right away, but are meant to punish players taking too long in a given level
    // There is never more than one flame on a given level. If a flame should spawn it is skipped instead.
    IEnumerator FlameSpawnLoop(int levelID, PlayerController player)
    {   
        yield return new WaitForSeconds(_flameData.FirstFlameDelay);
        while (player != null)
        {   

            var delay = _flameData.FlameSpawnDelayRange.RandomIntInclusive;
            yield return new WaitForSeconds(delay);
            yield return new WaitUntil(() => _spawningFlames);
            //yield return new WaitUntil(() => !player.IsJump); //* CONSIDER
            
            var spawnController = _flameSpawnControllers.OrderBy(fsc => Mathf.Abs(fsc.YLevel - player.Pos.y)).First();

            if (spawnController.ActiveFlame == null) // Prevent spawning a flame on the same level if there's already a flame present.
            {   
                var newFlame = InstantiateNPC(_flamePrefab);       
                spawnController.ActiveFlame = newFlame;                       
                newFlame.transform.position = spawnController.GetSpawn(player);
                newFlame.GetComponent<FlameController>().SetFacingTowards(player);
                newFlame.SetActive(true);
            }

            yield return new WaitUntil(() => spawnController.ActiveFlame == null || player == null); // Wait until that flame has been destroyed or the player has been removed before resuming the spawn loop
        }
    }

    GameObject InstantiateNPC(GameObject prefab)
    {
        GameObject newNPC = GameObject.Instantiate(prefab);
        newNPC.SetActive(false);

        if(_debugging) Debug.Log($"{this} spawned {newNPC}");

        if (newNPC.TryGetComponent<EnemyController>(out EnemyController enemy))
        {
            newNPC.transform.parent = _enemyGrouping;
            PresentEnemies.Add(enemy);
            _npcPlacementConveyor.Add(newNPC);
        } 
        else if (newNPC.TryGetComponent<CoinController>(out CoinController coin))
        {
            newNPC.transform.parent = _coinGrouping;
            PresentCoins.Add(coin);
            _npcPlacementConveyor.Add(newNPC);
        }
        else if (newNPC.TryGetComponent<FlameController>(out FlameController flame))
        {   
            newNPC.transform.parent = _flameGrouping;
            PresentFlames.Add(flame);
        }
        
        newNPC.name = $"{prefab.name} #{newNPC.GetInstanceID()}"; 

        return newNPC;
    }

    void SetNPCPosition(GameObject npc, Transform spawnTr)
    {   
        var idx = UnityEngine.Random.Range(0, _npcSpawnControllers.Count);
        npc.transform.position = (Vector2) spawnTr.position;
        npc.GetComponent<EntityController>().UpdateFacing((int) spawnTr.localScale.x); // Alternative: use an edgecollider2d in the spawn so they'd turn around. But I kind of like this hacky solution.
    }

    void DestroyAllNPC()
    {
        foreach (var item in PresentEnemies)
        {
            item.Delete();
        }

        foreach (var item in PresentCoins)
        {
            item.Delete();
        }

        foreach (var item in PresentFlames)
        {
            item.Delete();
        }

        PresentEnemies.Clear();
        PresentCoins.Clear();
        PresentFlames.Clear();
    }

    void TryCollectCoin(CoinController coinCollected, PlayerController byPlayer)
    {
        if (!coinCollected.CanBeCollected) return;

        coinCollected.OnCollect();
        GivePointsEvent?.Invoke(coinCollected.Points, byPlayer, PointTypeID.COIN);
        PresentCoins.Remove(coinCollected);
    }

    void TryFlipEnemy(EnemyController enemyHit, PlayerController byPlayer, bool ignoreVector=false)
    {
        if(_debugging) Debug.Log($"Received flip event for {enemyHit}.");

        if(!enemyHit.CanBeFlipped) return;

        var contactPoint = ignoreVector?Vector2.zero:(Vector2) byPlayer.Pos;
        enemyHit.OnFlip(contactPoint);
        if (enemyHit.GivePointsOnFlip) GivePointsEvent?.Invoke(enemyHit.FlipPoints, byPlayer, PointTypeID.FLIP);
    }

    #region event reactions
    void PlayerController_TryEnemyFlipEvent(EnemyController enemyHit, PlayerController byPlayer) => TryFlipEnemy (enemyHit, byPlayer);

    void PlayerController_TryCoinCollectEvent(CoinController coinCollected, PlayerController byPlayer) => TryCollectCoin(coinCollected, byPlayer);

    void PlayerController_EnemyKillEvent(EnemyController enemyHit, PlayerController byPlayer)
    {   
        if (enemyHit.IsDie)
        {
            Debug.LogError($"Received kill event for already dead {enemyHit}.");
            return;
        }

        enemyHit.OnDead((Vector2) byPlayer.Pos);
        GivePointsEvent?.Invoke(enemyHit.KillPoints, byPlayer, PointTypeID.KILL);
        PresentEnemies.Remove(enemyHit);

        if (_hasActiveEnemies)
            InstantiateNPC(_coinPrefab); //* CONSIDER using coroutine to add a delay to the spawn
        
        if(PresentEnemies.Count == 1 && _isFinishedSpawning)
            PresentEnemies[0].IncreaseAnger(99); // If only one enemy is left, they are always set to their highest anger level
    }

    void PlayerController_PowTriggeredEvent(PlayerController triggeringPlayer)
    {
        PresentEnemies.ForEach(enemy => TryFlipEnemy(enemy, triggeringPlayer));
        PresentCoins.ForEach(coin => TryCollectCoin(coin, triggeringPlayer));
        PresentFlames.ForEach(flame => flame.Delete());
    }

    void PlayerManager_NewPlayerJoinedEvent(PlayerController player) => StartCoroutine(FlameSpawnLoop(_currentLevelData.LevelID, player));

    void NPCExitController_NPCExitTeleportEvent(GameObject npc) 
    {
        npc.SetActive(false);
        if(npc.TryGetComponent<CoinController>(out CoinController coin))
        {
            PresentCoins.Remove(coin);
            Destroy(npc);
        } else 
        {
            _npcPlacementConveyor.Add(npc);
        }
    }

    void GameManager_LevelChangeEvent(LevelData data) 
    {
        if (!_isFinishedSpawning)
            Debug.LogError("Level change triggered despite enemies left to spawn");
        
        if (PresentEnemies.Count > 0)
            Debug.LogError("Level change triggered despite active enemies left.");

        _currentLevelData = data;
        _newEnemiesToSpawn.Clear();
        _currentLevelData.EnemiesToSpawn.ForEach(container => _newEnemiesToSpawn.Add(new EnemyPrefabContainer(container))); // Clone the prefab-list to avoid references
    }

    void GameManager_LevelStartedEvent() => StartCoroutine(EnemySpawnLoop(_currentLevelData.LevelID));
    
    #endregion
}