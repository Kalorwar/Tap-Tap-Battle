using _Project.Scripts.Core;
using _Project.Scripts.Enemies;
using _Project.Scripts.Events;
using DG.Tweening;
using R3;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class EnemyHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider _slider;
        [SerializeField] private RectTransform _fillRect;
        [SerializeField] private Image _fillImage;

        [Header("Chunk effect")] [SerializeField]
        private Image _damageChunkPrefab;

        [SerializeField] private Transform _chunkContainer;

        [Header("Animations")] [SerializeField]
        private float _shakeDuration = 0.15f;

        [SerializeField] private float _shakeStrength = 10f;
        [SerializeField] private float _chunkFadeDuration = 0.5f;
        [SerializeField] private float _chunkMoveDistance = 30f;
        [SerializeField] private int _defaultPoolSize = 5;
        [SerializeField] private int _maxPoolSize = 20;

        private readonly CompositeDisposable _disposables = new();
        private ObjectPool<Image> _chunkPool;
        private Vector2 _originalPosition;
        private Sequence _shakeSequence;

        private void Awake()
        {
            _chunkPool = new ObjectPool<Image>(
                CreateChunk,
                OnChunkGet,
                OnChunkRelease,
                OnChunkDestroy,
                false,
                _defaultPoolSize,
                _maxPoolSize
            );
        }

        private void Start()
        {
            _originalPosition = _fillRect.anchoredPosition;

            EnemyHitSignal.OnEnemyHit
                .Subscribe(OnHealthChanged)
                .AddTo(_disposables);

            LevelCompletedSignal.OnLevelCompleted
                .Subscribe(_ => RefreshBar())
                .AddTo(_disposables);
        }

        private void OnDestroy()
        {
            _disposables.Dispose();
            _shakeSequence?.Kill();
            _chunkPool?.Dispose();
        }

        private Image CreateChunk()
        {
            var chunk = Instantiate(_damageChunkPrefab, _chunkContainer);
            return chunk;
        }

        private void OnChunkGet(Image chunk)
        {
            chunk.gameObject.SetActive(true);
            chunk.color = new Color(
                _fillImage.color.r * 0.8f,
                _fillImage.color.g * 0.8f,
                _fillImage.color.b * 0.8f,
                0.6f
            );
        }

        private void OnChunkRelease(Image chunk)
        {
            DOTween.Kill(chunk.rectTransform);
            DOTween.Kill(chunk);
            chunk.gameObject.SetActive(false);
        }

        private void OnChunkDestroy(Image chunk)
        {
            Destroy(chunk.gameObject);
        }

        private void RefreshBar()
        {
            _slider.value = 1;
            _shakeSequence?.Kill();
            _fillRect.anchoredPosition = _originalPosition;
        }

        private void OnHealthChanged(EnemyHitEvent evt)
        {
            var toPercent = evt.CurrentHealthPercent;
            var fromPercent = toPercent + evt.DamagePercent;

            CreateDamageChunk(fromPercent, toPercent);

            _slider.value = toPercent;
            ShakeBar();
        }

        private void CreateDamageChunk(float fromPercent, float toPercent)
        {
            var chunk = _chunkPool.Get();
            var chunkRect = chunk.rectTransform;

            var barRect = _fillRect;
            var barWidth = barRect.rect.width;
            var barHeight = barRect.rect.height;
            var chunkWidth = (fromPercent - toPercent) * barWidth;
            var leftEdge = toPercent * barWidth;

            chunkRect.anchorMin = new Vector2(0, 0.5f);
            chunkRect.anchorMax = new Vector2(0, 0.5f);
            chunkRect.pivot = new Vector2(0, 0.5f);

            var barPos = barRect.anchoredPosition;
            chunkRect.anchoredPosition = new Vector2(barPos.x + leftEdge, barPos.y);
            chunkRect.sizeDelta = new Vector2(chunkWidth, barHeight);

            var chunkSequence = DOTween.Sequence();
            chunkSequence.AppendInterval(0.05f);
            chunkSequence.Join(chunk.DOFade(0f, _chunkFadeDuration));
            chunkSequence.Join(chunkRect.DOAnchorPosY(
                chunkRect.anchoredPosition.y - _chunkMoveDistance,
                _chunkFadeDuration).SetEase(Ease.OutQuad));

            chunkSequence.OnComplete(() => _chunkPool.Release(chunk));
        }

        private void ShakeBar()
        {
            _shakeSequence?.Kill();
            _fillRect.anchoredPosition = _originalPosition;

            _shakeSequence = DOTween.Sequence();
            _shakeSequence.Append(_fillRect.DOShakeAnchorPos(_shakeDuration, new Vector2(_shakeStrength, 0)));
            _shakeSequence.OnComplete(() => _fillRect.anchoredPosition = _originalPosition);
        }
    }
}