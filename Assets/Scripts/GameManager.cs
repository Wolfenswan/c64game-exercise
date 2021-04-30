using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using NEINGames.Singleton;

//DontDestroyOnLoad?
//Instead of different scenes, use a static variable incrementing id up to 25, then K.O.?
public class GameManager : Singleton<GameManager>
{
    public event Action<LevelData> LevelChangeEvent;
    public event Action LevelStartedEvent;

    [SerializeField] GameData _data;
    [SerializeField] int _currentLevel = 1;
    
    List<LevelData> _levels = new List<LevelData>();
    NPCManager _npcManager;
    PlayerManager _playerManager;

    public GameData Data{get => _data;}
    public LevelData CurrentLevelData{get => _levels.Single(data => data.LevelID == _currentLevel);}

    void Awake() 
    {   
        InitializeSingleton(this); // Might need additional check
        _npcManager = null; // FIND
        _playerManager = null; // FIND
        _levels = _data.Levels;
        // CONSIDER create NPCManager & PlayerManager programmatically as children of GameManager
    }

    void Start() 
    {
        // Play music jingle
        // Play level started event (maybe as coroutine?)
        LevelChangeEvent?.Invoke(CurrentLevelData);
        LevelStartedEvent?.Invoke();
    }

    void OnEnable() 
    {
        PlayerManager.Instance.AllPlayersKilledEvent += PlayerManager_AllPlayersKilledEvent;
        // Listen to GameOverEvent
        // Listen to LevelDoneEvent
    }

    void OnDisable() 
    {
        PlayerManager.Instance.AllPlayersKilledEvent -= PlayerManager_AllPlayersKilledEvent;
    }

    void Update() 
    {
        
    }

    void PlayerManager_AllPlayersKilledEvent()
    {
        // GameOverEvent
        // Move to Highscore-Screen
    }
    

    // LevelDoneEvent
    // Play Jingle
    // _curentLevel++;
    // If no next level exists; loop to 1?
    // Set to next Scene (LevelChangeEvent)
}