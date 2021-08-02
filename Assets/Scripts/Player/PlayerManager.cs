using UnityEngine;
using NEINGames.Singleton;
using System;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.InputSystem;

public class PlayerManager : Singleton<PlayerManager> // TODO probably does not need to be a singleton
{   
    public event Action<PlayerController> NewPlayerJoinedEvent;
    public event Action AllPlayersKilledEvent;
    public event Action PlayersTriggerPowEvent;
    public event Action PromptPauseEvent;
    public event Action PlayerContinueEvent;

    List<Transform> _playerSpawnPositions;
    List<Transform> _playerRespawnPositions;

    bool _canJoin = true;
    // Dictionary<int, PlayerController> _activePlayers = new Dictionary<int, PlayerController>();
    List<PlayerController> _activePlayers = new List<PlayerController>();

    public int PlayerCount{get => _activePlayers.Count;}

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

        PlayerController.PowTriggeredEvent += PlayerController_PowTriggeredEvent;

        NPCManager.GivePointsEvent += NPCManager_GivePointsEvent;
    }
    
    void OnDisable() 
    {   
        GameManager.Instance.LevelChangeEvent -= GameManager_LevelChangeEvent;
        GameManager.Instance.LevelStartedEvent -= GameManager_LevelStartedEvent;

        PlayerController.PowTriggeredEvent -= PlayerController_PowTriggeredEvent;

        NPCManager.GivePointsEvent -= NPCManager_GivePointsEvent;
    }

    // After joining players stay persistent for the entire session
    public void OnPlayerJoinedEvent(PlayerInput pI)
    {   
        //! TODO If there's no continue system -> prevent players from rejoining; _gameInProgress bool?
        if (!_canJoin)
            return;

        var idInt = pI.playerIndex;
        var idEnum = (PlayerID) idInt + 1;
        pI.gameObject.name = $"Player #{idInt+1}";
        pI.transform.parent = transform;

        var pc = pI.GetComponent<PlayerController>();
        pc.InitializePlayer(idEnum);
        _activePlayers.Add(pc);
        pc.SpawnAt(_playerSpawnPositions[idInt]);

        SubscribeToPlayerEvents(pc);

        NewPlayerJoinedEvent?.Invoke(pc);
    }

    // Called when changing to a new scene/level
    void ResetPlayerPositions()
    {
        _activePlayers.ForEach(player => player.SpawnAt(_playerSpawnPositions[player.IDInt]));
    }

    void SubscribeToPlayerEvents(PlayerController pc)
    {  
        pc.PlayerRespawnEvent += Player_PlayerRespawnEvent;
        pc.PlayerDeadEvent += Player_PlayerDeadEvent;
        pc.MoveOtherPlayerEvent += Player_MoveOtherPlayerEvent;
        pc.HopOtherPlayerEvent += Player_HopOtherPlayerEvent;
        pc.PlayerDisabledEvent += Player_PlayerDisabledEvent;
        pc.MenuButtonEvent += Player_StartButtonEvent;
    }

    void DesubscribeFromPlayerEvents(PlayerController pc)
    {
        pc.PlayerRespawnEvent -= Player_PlayerRespawnEvent;
        pc.PlayerDeadEvent -= Player_PlayerDeadEvent;
        pc.MoveOtherPlayerEvent -= Player_MoveOtherPlayerEvent;
        pc.HopOtherPlayerEvent -= Player_HopOtherPlayerEvent;
        pc.PlayerDisabledEvent -= Player_PlayerDisabledEvent;
        pc.MenuButtonEvent -= Player_StartButtonEvent;
    }

    void GameManager_LevelChangeEvent(LevelData data)
    {
        _activePlayers.ForEach(player => player.StopAllCoroutines());
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

    // Theoretically NPCManager could evoke AddPoints directly but using an event avoids tight coupling between NPCManager & Player-related classes
    void NPCManager_GivePointsEvent(int points, PlayerController receivingPlayer, PointTypeID type) => receivingPlayer.AddPoints(points, type);

    void PlayerController_PowTriggeredEvent(PlayerController byPlayer)
    {
        var affectedPlayers = _activePlayers.FindAll(player => player != byPlayer && player.CanHop);
        foreach (var player in affectedPlayers)
        {   
            player.ForceToHop();
        }
        PlayersTriggerPowEvent?.Invoke();
    }

    void Player_MoveOtherPlayerEvent(Vector2 moveVector, PlayerController otherPlayer) => otherPlayer.MoveStep(moveVector.x, moveVector.y, applyDeltaTime:true);

    void Player_HopOtherPlayerEvent(PlayerController otherPlayer, Vector2 contactPoint) => otherPlayer.ForceToHop();

    void Player_PlayerRespawnEvent(PlayerController player) => player.SpawnAt(_playerRespawnPositions[player.IDInt]);
    
    void Player_PlayerDeadEvent(PlayerController player)
    {
        //player.gameObject.SetActive(false);
        _activePlayers.Remove(player);

        // TODO before destroying: wait for continue prompt
        Destroy(player.gameObject);

        if (_activePlayers.Count == 0)
            AllPlayersKilledEvent?.Invoke();
    }

    void Player_PlayerDisabledEvent (PlayerController pc)
    {
        DesubscribeFromPlayerEvents(pc);

        // TODO test if object should be destroyed;
        // needs testing how PlayerInput registers rejoining etc.
    }

    void Player_StartButtonEvent()
    {
        PromptPauseEvent?.Invoke();
    }
}