using DG.Tweening;
using _Project.Scripts.Enemies;
using _Project.Scripts.Events;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    /// <summary>
    /// Spawns a burst of "magnetic" coins at the killed enemy position and flies them to the currency counter.
    /// </summary>
    public class CoinFlySystem : MonoBehaviour
    {
        private const float CoinSize = 10f;
        private const float PopDuration = 0.15f;
        private const float FadeDuration = 0.08f;

        private static readonly Color CoinColor = new Color32(255, 211, 77, 255); // #FFD34D

        [SerializeField] private RectTransform _targetRect;
        [SerializeField] private Canvas _canvas;
        [SerializeField] private int _coinsMin = 4;
        [SerializeField] private int _coinsMax = 5;
        [SerializeField] private float _spawnRadius = 22f;
        [SerializeField] private float _flightDuration = 0.5f;
        [SerializeField] private float _stagger = 0.03f;
        [SerializeField] private Sprite _coinSprite;

        private Enemy _currentEnemy;

        private void OnEnable()
        {
            EnemySpawnedSignal.OnEnemySpawned += OnEnemySpawned;
            EnemyKilledSignal.OnEnemyKilled += OnEnemyKilled;
        }

        private void OnDisable()
        {
            EnemySpawnedSignal.OnEnemySpawned -= OnEnemySpawned;
            EnemyKilledSignal.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemySpawned(EnemySpawnedInfo info)
        {
            _currentEnemy = info.Enemy;
        }

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            // Transform is still valid here (Destroy is deferred to the end of the frame).
            var enemy = _currentEnemy;
            _currentEnemy = null;

            if (enemy == null || _canvas == null || _targetRect == null)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var screenPoint = camera.WorldToScreenPoint(enemy.transform.position);
            if (screenPoint.z < 0f)
            {
                return;
            }

            var targetPoint = GetTargetPoint();
            var spawnPoint = (Vector2)screenPoint / _canvas.scaleFactor;
            var maxCoins = Mathf.Max(_coinsMin, _coinsMax);
            var coinCount = Random.Range(_coinsMin, maxCoins + 1);

            for (var i = 0; i < coinCount; i++)
            {
                SpawnCoin(spawnPoint, targetPoint, i * _stagger);
            }
        }

        // Points live in canvas units (screen px / scaleFactor), origin at the canvas bottom-left.
        private Vector2 GetTargetPoint()
        {
            // Target may sit in another canvas (e.g. ScreenSpaceCamera): convert its pivot to screen px first
            var targetCanvas = _targetRect.GetComponentInParent<Canvas>();
            var targetCamera = targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay
                ? targetCanvas.worldCamera
                : null;
            var screenPos = RectTransformUtility.WorldToScreenPoint(targetCamera, _targetRect.position);
            return screenPos / _canvas.scaleFactor;
        }

        private void SpawnCoin(Vector2 spawnPoint, Vector2 targetPoint, float delay)
        {
            var coin = new GameObject("CoinFly", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            var rect = (RectTransform)coin.transform;
            rect.SetParent(_canvas.transform, false);
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = new Vector2(CoinSize, CoinSize);
            rect.localScale = Vector3.zero;

            var start = spawnPoint + Random.insideUnitCircle * _spawnRadius;
            rect.anchoredPosition = start;

            var image = coin.GetComponent<Image>();
            image.sprite = _coinSprite; // null sprite renders as a plain white quad
            image.color = CoinColor;
            image.raycastTarget = false;

            var popPoint = start + Random.insideUnitCircle * (_spawnRadius * 0.5f);

            var sequence = DOTween.Sequence();
            sequence.AppendInterval(delay);
            sequence.Append(rect.DOScale(Vector3.one, PopDuration).SetEase(Ease.OutQuad));
            sequence.Join(rect.DOAnchorPos(popPoint, PopDuration).SetEase(Ease.OutQuad));
            sequence.Append(rect.DOAnchorPos(targetPoint, _flightDuration).SetEase(Ease.InQuad));
            sequence.Append(image.DOFade(0f, FadeDuration));
            sequence.OnComplete(() => Destroy(coin));
        }
    }
}
