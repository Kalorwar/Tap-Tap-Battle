using DG.Tweening;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.FX
{
    /// <summary>
    /// Short camera shake on every tap. Consecutive taps restart the shake without position drift.
    /// </summary>
    public class TapCameraShake : MonoBehaviour
    {
        [SerializeField] private float _duration = 0.1f;
        [SerializeField] private float _strength = 4f;

        private Vector3 _originalPosition;

        private void Awake()
        {
            _originalPosition = transform.position;
        }

        private void OnEnable()
        {
            TapEvent.OnTapped += OnTapped;
        }

        private void OnDisable()
        {
            TapEvent.OnTapped -= OnTapped;
            transform.DOKill();
            transform.position = _originalPosition;
        }

        private void OnTapped()
        {
            // Restart from the pristine position so shake offsets never accumulate.
            transform.DOKill();
            transform.position = _originalPosition;

            transform.DOShakePosition(_duration, _strength)
                .OnComplete(() => transform.position = _originalPosition);
        }
    }
}
