using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;
using NEINGames.Singleton;

public class GameManager : Singleton<GameManager>
{
    public event Action<LevelData> LevelChangeEvent;
    public event Action LevelStartedEvent;
    public event Action<int> UpdatePowState;

    [SerializeField] GameData _data;
    [SerializeField] int _currentLevelID = 1;
    
    List<LevelData> _levels = new List<LevelData>();
    NPCManager _npcManager;

    public GameData Data{get => _data;}
    public LevelData CurrentLevelData{get => _levels.Single(data => data.LevelID == _currentLevelID);}
    public int PowLeft{get; private set;} = 2; // Carries over from level to level and is reset every second bonus level

    void Awake() 
    {   
        InitializeSingleton(this);
        _npcManager = GameObject.FindGameObjectWithTag("NPCManager").GetComponent<NPCManager>();
        _levels = _data.Levels;
        //* CONSIDER create NPCManager & PlayerManager programmatically as children of GameManager
    }

    void Start() 
    {
        PlayerManager.Instance.AllPlayersKilledEvent += PlayerManager_AllPlayersKilledEvent;
        PlayerManager.Instance.PlayersTriggerPowEvent += PlayerManager_PowTriggeredEvent;
        _npcManager.AllEnemiesRemovedEvent += NPCManager_AllEnemiesRemovedEvent;

        StartLevel(_currentLevelID);
    }

    void OnDisable() 
    {
        PlayerManager.Instance.AllPlayersKilledEvent -= PlayerManager_AllPlayersKilledEvent;
        PlayerManager.Instance.PlayersTriggerPowEvent -= PlayerManager_PowTriggeredEvent;
        _npcManager.AllEnemiesRemovedEvent -= NPCManager_AllEnemiesRemovedEvent;
    }

    void StartLevel(int id)
    {   
        TogglePause(true);
        LevelChangeEvent?.Invoke(CurrentLevelData);
        UpdatePowState?.Invoke(PowLeft);
        // Play music jingle
        // Wait till jingle has played
        TogglePause(false); // TODO maybe requires coroutine & waitUntil properly loaded; waiting for PlayerManager & NPCManager to sent a done signal
        LevelStartedEvent?.Invoke(); 
        
    }

    void EndLevel()
    {
        TogglePause(true);
        // TODO Play Jingle
        _currentLevelID++;
        // Wait till Jingle has played
        // StartLevel(CurrentLevelData.ID);
    }

    void GameOver()
    {
        // TODO
        // play jingle?
        // change to highscore screen
        Debug.LogWarning("Game over!");
    }

    void CameraShake()
    {

    }

    void TogglePause(bool pauseEnabled) => Time.timeScale = pauseEnabled?0:1;

    void TogglePause() => Time.timeScale = Time.timeScale == 0?1:0;

    void PlayerManager_AllPlayersKilledEvent()=> GameOver();

    void PlayerManager_PowTriggeredEvent()
    {
        PowLeft -= 1;
        if(PowLeft >= 0)
        {
            // TODO Camera Shake
            UpdatePowState?.Invoke(PowLeft);
        } else if(PowLeft < 0) // Sanity check. Should not be possible to reduce POW below 0 as POWController disables itself before that.
        {
            Debug.LogWarning("GameManager: PowLeft has been reduced below 0.");
        }
    }

    void NPCManager_AllEnemiesRemovedEvent() => EndLevel();
    
}