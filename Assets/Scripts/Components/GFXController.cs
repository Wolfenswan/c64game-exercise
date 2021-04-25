
using UnityEngine;
using System;

[RequireComponent (typeof(SpriteRenderer), typeof(Animator))]
public class GFXController : MonoBehaviour {

    public event Action<int> AnimationStartedEvent;
    public event Action<int> AnimationFinishedEvent;

    Animator _animator;
    SpriteRenderer _spriteRenderer;
    int _currentAnimationHash;

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
        _animator.Play(newAnimationHash);
        _currentAnimationHash = newAnimationHash;
    }

    public void FlipSprite(bool x = false, bool y = false) {

        if (x)
            _spriteRenderer.flipX = !_spriteRenderer.flipX;

        if (y)
            _spriteRenderer.flipY = !_spriteRenderer.flipY;
    }

    public void SetAnimatorFloat(string name, float value) => _animator.SetFloat(name, value);

    void RaiseAnimationStartedEvent(AnimationEvent animEvent) {
        int animHash = animEvent.animatorStateInfo.shortNameHash;
        AnimationStartedEvent?.Invoke(animHash);
    }

    void RaiseAnimationFinishedEvent(AnimationEvent animEvent) {
        int animHash = animEvent.animatorStateInfo.shortNameHash;
        AnimationFinishedEvent?.Invoke(animHash);
    }
}