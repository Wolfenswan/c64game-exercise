using UnityEngine;
using NEINGames.Singleton;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : Singleton<PlayerManager> // TODO probably does not need to be a singleton
{
    public event Action AllPlayersKilledEvent;

    [SerializeField] PlayerData _data;
    [SerializeField] GameObject _playerPrefab;

    List<Transform> _playerSpawnPositions;
    List<Transform> _playerRespawnPositions;

    bool _canJoin = true;
    Dictionary<int, PlayerController> _activePlayers = new Dictionary<int, PlayerController>();

    void Awake() 
    {
        InitializeSingleton(this);
        _playerSpawnPositions = GameObject.FindGameObjectsWithTag("PlayerSpawn").Select(g => g.transform).ToList();
        _playerRespawnPositions = GameObject.FindGameObjectsWithTag("PlayerRespawn").Select(g => g.transform).ToList();
    }

    void OnEnable() 
    {
    }

    void Start() 
    {
        GameManager.Instance.LevelChangeEvent += GameManager_LevelChangeEvent; // Subscribing in OnEnable can cause a NullReferenceException
        GameManager.Instance.LevelStartedEvent += GameManager_LevelStartedEvent;
    }
    
    void OnDisable() 
    {   
        GameManager.Instance.LevelChangeEvent -= GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent -= GameManager_LevelStartedEvent;
    }

    // After joining players stay persistent for the entire session
    public void OnPlayerJoinedEvent(PlayerInput pI)
    {   
        //! If there's no continue system -> prevent players from rejoining; _gameInProgress bool?
        if (!_canJoin)
            return;

        var id = pI.playerIndex;
        pI.gameObject.name = $"Player #{id}";
        pI.transform.parent = transform;

        var pc = pI.GetComponent<PlayerController>();
        pc.InitializePlayer(id);
        _activePlayers.Add(id, pc);
        pc.PlaceAtSpawn(_playerSpawnPositions[id]);

        SubscribeToPlayerEvents(pc);
    }

    // Called when changing to a new scene/level
    void ResetPlayerPositions()
    {
        foreach (var item in _activePlayers.Values)
        {
            item.PlaceAtSpawn(_playerSpawnPositions[item.ID]);
        }
    }

    void SubscribeToPlayerEvents(PlayerController pc)
    {
        pc.PlayerRespawnEvent += Player_PlayerRespawnEvent;
        pc.PlayerDeadEvent += Player_PlayerDeadEvent;
        pc.MoveOtherPlayerEvent += Player_MoveOtherPlayerEvent;
        pc.PlayerDisabledEvent += Player_PlayerDisabledEvent;
    }

    void DesubscribeFromPlayerEvents(PlayerController pc)
    {
        pc.PlayerRespawnEvent -= Player_PlayerRespawnEvent;
        pc.PlayerDeadEvent -= Player_PlayerDeadEvent;
        pc.MoveOtherPlayerEvent -= Player_MoveOtherPlayerEvent;
        pc.PlayerDisabledEvent -= Player_PlayerDisabledEvent;
    }

    void GameManager_LevelChangeEvent(LevelData data)
    {
        // TODO maybe requires CoRoutine OnLevelChange
        // Disable Players for the time being
        // Make sure players cant join
        // Wait for new level to load
        ResetPlayerPositions(); // SpawnPlayers() at the default positions
    }

    void GameManager_LevelStartedEvent()
    {
        // Enable player controls; however might be unnecessary if TimeScale works as intended?
    }

    void Player_MoveOtherPlayerEvent(Vector2 moveVector, PlayerController otherPlayer) => otherPlayer.MoveStep(moveVector.x, moveVector.y);

    void Player_PlayerRespawnEvent(PlayerController player) => player.PlaceAtSpawn(_playerRespawnPositions[player.PlayerValues.ID]);
    
    void Player_PlayerDeadEvent(PlayerController player)
    {
        //player.gameObject.SetActive(false);
        _activePlayers.Remove(player.PlayerValues.ID);
        Destroy(player.gameObject);
        
        //! IDEA show "continue?" prompt on UI

        if (_activePlayers.Count == 0)
            AllPlayersKilledEvent?.Invoke();
    }

    void Player_PlayerDisabledEvent (PlayerController pc)
    {
        DesubscribeFromPlayerEvents(pc);

        // TODO test if object should be destroyed;
        // needs testing how PlayerInput registers rejoining etc.
    }
}