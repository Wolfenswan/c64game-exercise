
using UnityEngine;
using System;

[RequireComponent (typeof(SpriteRenderer), typeof(Animator))]
public class GFXController : MonoBehaviour {

    public event Action<int> AnimationStartedEvent;
    public event Action<int> AnimationFinishedEvent;

    Animator _animator = null;
    SpriteRenderer _spriteRenderer = null;
    int _currentAnimationHash;

    public Color Color {get=>_spriteRenderer.color;}
    public float SpriteWidth{get => _spriteRenderer.sprite.border.x + _spriteRenderer.sprite.border.z;}
    public bool SpriteIsVisible{get => _spriteRenderer.isVisible;}
    public bool SpriteXFlipped{get => _spriteRenderer.flipX;}
    public bool SpriteYFlipped{get => _spriteRenderer.flipY;}

    private void Awake() 
    {
        _spriteRenderer = GetComponent<SpriteRenderer>();
        _animator = GetComponent<Animator>();
    }

    public void ChangeAnimation (int newAnimationHash) 
    {
        if (_currentAnimationHash == newAnimationHash) return;
        _animator?.Play(newAnimationHash);
        _currentAnimationHash = newAnimationHash;
    }

    public void FlipSprite(bool x = false, bool y = false) {

        if (x)
            _spriteRenderer.flipX = !_spriteRenderer.flipX;

        if (y)
            _spriteRenderer.flipY = !_spriteRenderer.flipY;
    }

    public void SetSpriteColor(Color color)
    {
        _spriteRenderer.color = color;
    } 

    public void SetAnimatorVariable(string name, dynamic value) 
    {
        var type = value.GetType();
        if (type == typeof(bool))
            _animator.SetBool(name, value);
        else if (type == typeof(int))
            _animator.SetInteger(name, value);
        else if (type == typeof(float))
            _animator.SetFloat(name, value);
        else
            Debug.LogWarning($"Unknown value type {value.GetType()}");
    }

    void RaiseAnimationStartedEvent(AnimationEvent animEvent) {
        int animHash = animEvent.animatorStateInfo.shortNameHash;
        AnimationStartedEvent?.Invoke(animHash);
    }

    void RaiseAnimationFinishedEvent(AnimationEvent animEvent) {
        int animHash = animEvent.animatorStateInfo.shortNameHash;
        AnimationFinishedEvent?.Invoke(animHash);
    }
}