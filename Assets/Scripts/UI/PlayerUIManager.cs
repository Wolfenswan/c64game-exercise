using UnityEngine;
using System.Collections.Generic;

// The PlayerUIManager maintains a dictionary with the UI-displays for each player
// It receives events from PlayerValues with updated score and live values
// It passes these new values on to the respective UI-Display
public class PlayerUIManager : MonoBehaviour 
{
    Dictionary<PlayerID, PlayerUIController> _playerUIController = new Dictionary<PlayerID, PlayerUIController>();

    void Start() 
    {
        foreach (Transform child in transform)
        {
            if (child.TryGetComponent(out PlayerUIController uiDisplay))
                _playerUIController[uiDisplay.PlayerID] = uiDisplay;
        }

        PlayerValues.ScoreUpdatedEvent += PV_ScoreUpdatedEvent;
        PlayerValues.LivesUpdatedEvent += PV_LivesUpdatedEvent;
    }

    void PV_ScoreUpdatedEvent(PlayerID id, int score) => _playerUIController[id].UpdateScore(score);
    void PV_LivesUpdatedEvent(PlayerID id, int newLives) => _playerUIController[id].UpdateLives(newLives);
}