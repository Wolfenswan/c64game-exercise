using UnityEngine;
using System;
using System.Collections;
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

    public Camera Camera{get; private set;}
    public GameData Data{get => _data;}
    public LevelData CurrentLevelData{get => _levels.Single(data => data.LevelID == _currentLevelID);}
    public int PowLeft{get; private set;} = 2; // Carries over from level to level and is reset every second bonus level

    bool _cameraIsShaking = false;
    bool _playerHasJoined = false; // Is toggled to true after the first player has joined. Only then will the level finish loading and unpause.

    void Awake() 
    {   
        InitializeSingleton(this);
        Camera = Camera.main; // Caching expensive lookup
        _npcManager = GameObject.FindGameObjectWithTag("NPCManager").GetComponent<NPCManager>();
        _levels = _data.Levels;
        //* CONSIDER create NPCManager & PlayerManager programmatically as children of GameManager
    }

    void Start() 
    {
        PlayerManager.Instance.NewPlayerJoinedEvent += PlayerManager_PlayersAreActive;
        PlayerManager.Instance.AllPlayersKilledEvent += PlayerManager_AllPlayersKilledEvent;
        PlayerManager.Instance.PlayersTriggerPowEvent += PlayerManager_PowTriggeredEvent;
        PlayerManager.Instance.PromptPauseEvent += PlayerManager_PauseEvent;

        _npcManager.AllEnemiesRemovedEvent += NPCManager_AllEnemiesRemovedEvent;
        
        StartCoroutine(LoadLevel(_currentLevelID));
    }

    void OnDisable() 
    {
        PlayerManager.Instance.NewPlayerJoinedEvent -= PlayerManager_PlayersAreActive;
        PlayerManager.Instance.AllPlayersKilledEvent -= PlayerManager_AllPlayersKilledEvent;
        PlayerManager.Instance.PlayersTriggerPowEvent -= PlayerManager_PowTriggeredEvent;   
        PlayerManager.Instance.PromptPauseEvent -= PlayerManager_PauseEvent;

        _npcManager.AllEnemiesRemovedEvent -= NPCManager_AllEnemiesRemovedEvent;
    }

    void GameOver()
    {
        // TODO
        // play jingle?
        // change to highscore screen
        Debug.LogWarning("Game over!");
    }

    IEnumerator EndLevel()
    {   
        TogglePause(true);
        yield return null; // TODO play & wait for endjingle
        _currentLevelID++;
        // StartCoroutine(LoadLevel(_currentLevelID));
    }

    IEnumerator LoadLevel(int levelID)
    {
        LevelChangeEvent?.Invoke(CurrentLevelData);
        UpdatePowState?.Invoke(PowLeft);
        yield return new WaitUntil(() => _playerHasJoined);
        // TODO Play music jingle
        // Wait till jingle has played
        LevelStartedEvent?.Invoke(); 
    }

    IEnumerator CameraShake()
    {
        // todo move to fields/data and tweak values
        
        if (_cameraIsShaking) yield return null;

        _cameraIsShaking = true;
        var targetPos = Camera.transform.position + new Vector3(0f,_data.CameraShakeStrength,0f);
        var origPos = Camera.transform.position;
        var speed = _data.CameraShakeSpeed;
        while(Camera.transform.position != targetPos)
        {   
            Camera.transform.position = Vector3.MoveTowards(Camera.transform.position, targetPos, speed * Time.deltaTime);
            yield return new WaitForEndOfFrame();
            if (Camera.transform.position == targetPos) targetPos = origPos;
        }

        _cameraIsShaking = false;
    }

    void TogglePause(bool pauseEnabled) => Time.timeScale = pauseEnabled?0:1;

    void TogglePause() => Time.timeScale = Time.timeScale == 0?1:0;

    void PlayerManager_PlayersAreActive(PlayerController player) => _playerHasJoined = true;

    void PlayerManager_AllPlayersKilledEvent()=> GameOver();

    void PlayerManager_PowTriggeredEvent()
    {
        if (!_data.InfinitePow) PowLeft -= 1;
        if(PowLeft >= 0)
        {
            StartCoroutine(CameraShake());
            UpdatePowState?.Invoke(PowLeft);
        } else if(PowLeft < 0) // Sanity check. Should not be possible to reduce POW below 0 as POWController disables itself before that.
        {
            Debug.LogWarning("GameManager: PowLeft has been reduced below 0.");
        }
    }
    void PlayerManager_PauseEvent()
    {
        TogglePause();
    } 

    void NPCManager_AllEnemiesRemovedEvent() => StartCoroutine(EndLevel());
    
}