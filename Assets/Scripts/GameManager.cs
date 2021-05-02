using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using NEINGames.Singleton;

public class GameManager : Singleton<GameManager>
{
    public event Action<LevelData> LevelChangeEvent;
    public event Action LevelStartedEvent;

    [SerializeField] GameData _data;
    [SerializeField] int _currentLevelID = 1;
    
    List<LevelData> _levels = new List<LevelData>();
    NPCManager _npcManager;
    //PlayerManager _playerManager;

    public GameData Data{get => _data;}
    public LevelData CurrentLevelData{get => _levels.Single(data => data.LevelID == _currentLevelID);}

    void Awake() 
    {   
        InitializeSingleton(this); // Might need additional check
        _npcManager = GameObject.FindGameObjectWithTag("NPCManager").GetComponent<NPCManager>(); // FIND
        //_playerManager = GameObject.FindGameObjectWithTag("PlayerManager").GetComponent<PlayerManager>();; // FIND
        _levels = _data.Levels;
        // CONSIDER create NPCManager & PlayerManager programmatically as children of GameManager
    }

    void Start() 
    {
        PlayerManager.Instance.AllPlayersKilledEvent += PlayerManager_AllPlayersKilledEvent;
        
        _npcManager.AllEnemiesRemovedEvent += NPCManager_AllEnemiesRemovedEvent;
        StartLevel();
    }

    void OnEnable() 
    {
    }

    void OnDisable() 
    {
        PlayerManager.Instance.AllPlayersKilledEvent -= PlayerManager_AllPlayersKilledEvent;

        _npcManager.AllEnemiesRemovedEvent -= NPCManager_AllEnemiesRemovedEvent;
    }

    void StartLevel()
    {   
        TogglePause(true);
        LevelChangeEvent?.Invoke(CurrentLevelData);
        // Play music jingle
        TogglePause(false); // TODO maybe requires coroutine & waitUntil properly loaded; waiting for PlayerManager & NPCManager to sent a done signal
        LevelStartedEvent?.Invoke(); 
        
    }

    void EndLevel()
    {
        // Play Jingle
        TogglePause(true);
        _currentLevelID++;
        LevelChangeEvent(CurrentLevelData);
        TogglePause(false); // TODO again, probably coroutine required to make sure the audio plays; then set TimeScale to 0 and to 1 again after the level change
    }

    void GameOver()
    {
        // TODO
        // play jingle?
        // change to highscore screen
    }

    void TogglePause(bool pauseEnabled) => Time.timeScale = pauseEnabled?0:1;

    void PlayerManager_AllPlayersKilledEvent()=> GameOver();

    void NPCManager_AllEnemiesRemovedEvent() => EndLevel();
    
}