using System;
using System.Collections;
using DG.Tweening;
using UnityEngine;

public sealed class RodView : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private Transform _rotationRoot;
    [SerializeField] private Animator _animator;
    [SerializeField] private AnimationClip _bendClip;

    [Header("Animator")]
    [SerializeField] private string _bendStateName = "Base Layer.RodBend";

    [SerializeField, Range(0f, 1f)] private float _holdNormalizedTime = 0.5f;

    [Header("Whole rod rotation")]
    [SerializeField] private Vector3 _windUpRotationOffset = new(-20f, 0f, 0f);

    [SerializeField] private Vector3 _castRotationOffset = new(30f, 0f, 0f);

    [Header("Durations")]
    [SerializeField, Min(0.01f)] private float _windUpDuration = 0.25f;
    [SerializeField, Min(0.01f)] private float _castDuration = 0.15f;
    [SerializeField, Min(0.01f)] private float _returnDuration = 0.3f;

    public event Action CastReleased;
    public event Action Returned;

    private Quaternion _defaultLocalRotation;
    private int _bendStateHash;

    private Coroutine _routine;
    private Tween _rotationTween;

    private void Awake()
    {
        if (_rotationRoot == null ||
            _animator == null ||
            _bendClip == null)
        {
            Debug.LogError("[RodView] Назначить ссылки!");

            enabled = false;
            return;
        }

        _defaultLocalRotation = _rotationRoot.localRotation;
        _bendStateHash = Animator.StringToHash(_bendStateName);

        SetBendPose(0f);
    }

    public void PlayCast()
    {
        RestartRoutine(CastRoutine());
    }

    public void PlayReturn()
    {
        RestartRoutine(ReturnRoutine());
    }

    private IEnumerator CastRoutine()
    {
        SetBendPose(0f);

        Quaternion windUpRotation = _defaultLocalRotation * Quaternion.Euler(_windUpRotationOffset);

        _rotationTween = _rotationRoot
            .DOLocalRotateQuaternion(
                windUpRotation,
                _windUpDuration)
            .SetEase(Ease.OutSine);

        yield return _rotationTween.WaitForCompletion();

        Quaternion castRotation =_defaultLocalRotation * Quaternion.Euler(_castRotationOffset);

        _rotationTween = _rotationRoot
            .DOLocalRotateQuaternion(
                castRotation,
                _castDuration)
            .SetEase(Ease.OutCubic);

        CastReleased?.Invoke();

        yield return PlayBendSegment(
            from: 0f,
            to: _holdNormalizedTime,
            duration: _castDuration);

        _routine = null;
    }

    private IEnumerator ReturnRoutine()
    {
        SetBendPose(_holdNormalizedTime);

        _rotationTween = _rotationRoot
            .DOLocalRotateQuaternion(
                _defaultLocalRotation,
                _returnDuration)
            .SetEase(Ease.OutSine);

        yield return PlayBendSegment(
            from: _holdNormalizedTime,
            to: 1f,
            duration: _returnDuration);

        if (_rotationTween != null &&
            _rotationTween.IsActive())
        {
            yield return _rotationTween.WaitForCompletion();
        }

        Returned?.Invoke();
        _routine = null;
    }

    private IEnumerator PlayBendSegment(
        float from,
        float to,
        float duration)
    {
        SetBendPose(from);

        float normalizedLength = to - from;
        float segmentLength = _bendClip.length * normalizedLength;

        _animator.speed = segmentLength / Mathf.Max(0.01f, duration);

        while (_animator
               .GetCurrentAnimatorStateInfo(0)
               .normalizedTime < to)
        {
            yield return null;
        }

        SetBendPose(to);
    }

    private void SetBendPose(float normalizedTime)
    {
        _animator.speed = 0f;

        _animator.Play(
            _bendStateHash,
            layer: 0,
            normalizedTime: normalizedTime);

        _animator.Update(0f);
    }

    private void RestartRoutine(IEnumerator routine)
    {
        StopCurrentAnimation();
        _routine = StartCoroutine(routine);
    }

    private void StopCurrentAnimation()
    {
        if (_routine != null)
        {
            StopCoroutine(_routine);
            _routine = null;
        }

        _rotationTween?.Kill();
        _rotationTween = null;

        if (_animator != null)
            _animator.speed = 0f;
    }

    private void OnDestroy()
    {
        _rotationTween?.Kill();
    }
}