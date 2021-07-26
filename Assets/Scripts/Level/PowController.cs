using UnityEngine;
using TMPro;
using System.Collections.Generic;

[RequireComponent(typeof (SpriteRenderer))]
public class PowController : MonoBehaviour 
{
    [SerializeField] Sprite _defaultSprite;
    [SerializeField] Sprite _usedSprite;
    [SerializeField] Sprite _emptySprite = null;

    Dictionary<int, Sprite> _spriteAtUsecount = new Dictionary<int, Sprite>();

    SpriteRenderer _spriteRenderer;
    int _usesLeft;

    void Awake() 
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _spriteAtUsecount[2] = _defaultSprite;
        _spriteAtUsecount[1] = _usedSprite;
        _spriteAtUsecount[0] = _emptySprite;
    }

    void Start() 
    {
        _usesLeft = GameManager.Instance.PowLeft;
        transform.Find("DebugText").GetComponent<TextMeshPro>().text = $"{this}\n{_usesLeft}";
        GameManager.Instance.UpdatePowState += GameManager_UpdatePowState;
    }

    void OnDisable() 
    {
        GameManager.Instance.UpdatePowState -= GameManager_UpdatePowState;
    }

    void GameManager_UpdatePowState(int powLeft)
    {
        _usesLeft = powLeft;
        transform.Find("DebugText").GetComponent<TextMeshPro>().text = $"{this}\n{_usesLeft}";
        Sprite sprite;
        if(_spriteAtUsecount.TryGetValue(_usesLeft, out sprite))
            _spriteRenderer.sprite = sprite;
        else
            _spriteRenderer.sprite = null;

        if (powLeft == 0)
            gameObject.SetActive(false);
    }
}