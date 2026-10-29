using System;
using DG.Tweening;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class HealthBarShaker : IDisposable
    {
        private readonly float _duration;
        private readonly Vector2 _originalPosition;
        private readonly float _strength;
        private readonly RectTransform _target;

        private Sequence _sequence;

        public HealthBarShaker(
            RectTransform target,
            float duration = 0.15f,
            float strength = 10f)
        {
            _target = target;
            _originalPosition = target.anchoredPosition;
            _duration = duration;
            _strength = strength;
        }

        public void Dispose()
        {
            _sequence?.Kill();
        }

        public void Play()
        {
            _sequence?.Kill();
            _target.anchoredPosition = _originalPosition;

            _sequence = DOTween.Sequence();
            _sequence.Append(
                _target.DOShakeAnchorPos(_duration, new Vector2(_strength, 0)));
            _sequence.OnComplete(() =>
                _target.anchoredPosition = _originalPosition);
        }

        public void Reset()
        {
            _sequence?.Kill();
            _target.anchoredPosition = _originalPosition;
        }
    }
}