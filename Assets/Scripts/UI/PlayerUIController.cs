// TMP-Children:
// Player name display (turn PlayerID to string? or set in editor directly?)
// Score tracker (also: "Press Button to join")
// Live tracker (below name)
// Show multiplier???

using UnityEngine;
using TMPro;

public class PlayerUIController : MonoBehaviour 
{
    [SerializeField] PlayerID _playerID;
    [SerializeField] string _playerHasNotJoinedText; //* Could also be stored in GameData?
    [SerializeField] TextMeshPro _playerNameUI;
    [SerializeField] TextMeshPro _scoreUI;
    [SerializeField] TextMeshPro _livesUI;
    [SerializeField] Sprite _liveSprite;

    public PlayerID PlayerID {get => _playerID;}

    string _playerName;

    void Awake() 
    {
        _playerName = _playerNameUI.text;
        ResetUIText();
    }

    void ResetUIText()
    {   
        _playerNameUI.text = _playerHasNotJoinedText;
        _livesUI.text = "";
        _scoreUI.text = "";
    }

    public void UpdateScore(int newScore)
    {
        if (_playerNameUI.text == _playerHasNotJoinedText)
            _playerNameUI.text = _playerName;
        _scoreUI.text = $"Score: {newScore.ToString("D8")}";
    }

    public void UpdateLives(int liveCount)
    {
        //! To be replaced with a sprite/icon system later
        
        _livesUI.text = $"Lives: {liveCount}";

        if (liveCount == 0)
            ResetUIText();
    }
}

/*
void CreateIcons(int number)
    {   
        if (number == 0)
        {
            Destroy(_decoyIcon);
            return;
        }

        var width = _decoyIcon.GetComponent<RectTransform>().rect.width;
        var posX = _decoyIcon.GetComponent<RectTransform>().anchoredPosition.x;
        var anchoredPosTempVector = new Vector2 (posX, 0);
        for (int i = 1; i < number; i++)
        {
            var newIcon = Instantiate(_decoyIcon, new Vector3(0,0,0), Quaternion.identity, this.transform);
            var rT = newIcon.GetComponent<RectTransform>();
            anchoredPosTempVector.x += posX;
            rT.localPosition = new Vector3 (0,0,0);
            rT.anchoredPosition = anchoredPosTempVector;

            _decoyIcons.Add(newIcon);
        }
    }

    void DeleteLastIcon()
    {
        var icon = _decoyIcons.Last();
        _decoyIcons.Remove(icon);
        Destroy(icon);
    }
*/