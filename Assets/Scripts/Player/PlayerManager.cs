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

    Dictionary<int, PlayerController> _activePlayers = new Dictionary<int, PlayerController>();

    void Awake() 
    {
        InitializeSingleton(this);
        _playerSpawnPositions = GameObject.FindGameObjectsWithTag("PlayerSpawn").Select(g => g.transform).ToList();
        _playerRespawnPositions = GameObject.FindGameObjectsWithTag("PlayerRespawn").Select(g => g.transform).ToList();
    }

    void OnEnable() 
    {
        // PlayerController.PlayerRespawnEvent += PlayerController_PlayerRespawnEvent;
        // PlayerController.PlayerDeadEvent += PlayerController_PlayerDeadEvent; //PlayerDeadEvent is fired once a player has lost all their lives
    }

    void Start() 
    {
        GameManager.Instance.LevelChangeEvent += GameManager_LevelChangeEvent; // Subscribing in OnEnable can cause a NullReferenceException
    }
    
    void OnDisable() 
    {   
        // PlayerController.PlayerRespawnEvent -= PlayerController_PlayerRespawnEvent;
        // PlayerController.PlayerDeadEvent -= PlayerController_PlayerDeadEvent;

        GameManager.Instance.LevelChangeEvent -= GameManager_LevelChangeEvent;
    }

    // After joining players stay persistent for the entire session
    // TODO might need rework if hotseat keyboard inputs are required
    // See https://forum.unity.com/threads/multiple-players-on-keyboard-new-input-system.725834/
    // & https://forum.unity.com/threads/keyboard-splitter-local-multiplayer-keyboard.874135/
    public void OnPlayerJoinedEvent(PlayerInput pI)
    {   
        // TODO Later: check if already in level and then add desireable behaviour (join in progress or not)
        var id = pI.playerIndex;
        pI.gameObject.name = $"Player #{id}";
        pI.transform.parent = transform; // TODO needs testing; might not be the ideal solution for changing scenes.

        var pc = pI.GetComponent<PlayerController>();
        pc.InitializePlayer(id);
        _activePlayers.Add(id, pc);
        pc.PlaceAtSpawn(_playerSpawnPositions[id]);

        SubscribeToPlayerEvents(pc);
    }

    // Called when changing to a new scene/level
    void PlacePlayers()
    {
        var idx = 0;
        foreach (var item in _activePlayers.Values)
        {
            item.PlaceAtSpawn(_playerSpawnPositions[idx]);
            idx++;
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
        // Do stuff with the players; disable controls?
        // Wait for new level to load
        // SpawnPlayers()
    }

    void Player_MoveOtherPlayerEvent(Vector2 moveVector, PlayerController otherPlayer) => otherPlayer.MoveStep(moveVector.x, moveVector.y);

    void Player_PlayerRespawnEvent(PlayerController player) => player.PlaceAtSpawn(_playerRespawnPositions[player.PlayerValues.ID]);
    
    void Player_PlayerDeadEvent(PlayerController player)
    {
        player.gameObject.SetActive(false);
        _activePlayers.Remove(player.PlayerValues.ID);
        //* Other options: show continue? prompt on UI

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