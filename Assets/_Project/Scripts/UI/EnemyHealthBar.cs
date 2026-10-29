using _Project.Scripts.Core;
using _Project.Scripts.Enemies;
using _Project.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [Header("View")] [SerializeField] private Slider _slider;

        [SerializeField] private RectTransform _fillRect;
        [SerializeField] private Image _fillImage;

        [Header("Chunk pool")] [SerializeField]
        private Image _damageChunkPrefab;

        [SerializeField] private Transform _chunkContainer;
        [SerializeField] private int _defaultPoolSize = 5;
        [SerializeField] private int _maxPoolSize = 20;

        [Header("Chunk animation")] [SerializeField]
        private float _chunkFadeDuration = 0.5f;

        [SerializeField] private float _chunkFlashDuration = 0.1f;
        [SerializeField] private float _chunkMoveDistance = 30f;

        [Header("Shake")] [SerializeField] private float _shakeDuration = 0.15f;

        [SerializeField] private float _shakeStrength = 10f;

        private DamageChunkPool _chunkPool;
        private DamageChunkAnimationSettings _chunkSettings;
        private HealthBarShaker _shaker;

        private HealthBarView _view;

        private void Awake()
        {
            _view = new HealthBarView(_slider);

            _chunkPool = new DamageChunkPool(_damageChunkPrefab, _chunkContainer, _fillImage.color, _defaultPoolSize,
                _maxPoolSize);

            _chunkSettings =
                new DamageChunkAnimationSettings(_chunkFlashDuration, _chunkFadeDuration, _chunkMoveDistance);
        }

        private void Start()
        {
            _shaker = new HealthBarShaker(
                _fillRect,
                _shakeDuration,
                _shakeStrength);
        }

        private void OnEnable()
        {
            EnemyHitSignal.OnEnemyHit += OnHealthChanged;
            LevelCompletedSignal.OnLevelCompleted += OnLevelCompleted;
        }

        private void OnDisable()
        {
            EnemyHitSignal.OnEnemyHit -= OnHealthChanged;
            LevelCompletedSignal.OnLevelCompleted -= OnLevelCompleted;
        }

        private void OnDestroy()
        {
            _shaker?.Dispose();
            _chunkPool?.Dispose();
        }

        private void OnHealthChanged(EnemyHitEvent evt)
        {
            var to = evt.CurrentHealthPercent;
            var from = to + evt.DamagePercent;

            var chunk = _chunkPool.Get();
            DamageChunkAnimator.PlayChunkAnimation(
                chunk, _fillRect, from, to, _chunkSettings, _chunkPool);

            _view.SetPercent(to);
            _shaker.Play();
        }

        private void OnLevelCompleted(int level)
        {
            _view.ResetToFull();
            _shaker.Reset();
        }
    }
}
