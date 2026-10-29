using DG.Tweening;
using _Project.Scripts.Core;
using _Project.Scripts.Enemies;
using _Project.Scripts.Events;
using TMPro;
using UnityEngine;

namespace _Project.Scripts.UI
{
    public class DamagePopupSystem : MonoBehaviour
    {
        private const string TextFormat = "-{0}";
        private const float MinDamageToShow = 0.01f;
        private const float CriticalDamagePercent = 0.25f;

        private static readonly Color NormalColor = Color.white;
        private static readonly Color CriticalColor = new Color32(255, 165, 0, 255); // orange

        [SerializeField] private Canvas _canvas;
        [SerializeField] private float _lifetime = 0.7f;
        [SerializeField] private float _riseDistance = 60f;
        [SerializeField] private float _fontSize = 36f;
        [SerializeField] private float _xSpread = 30f;
        [SerializeField] private float _yOffset = 40f;

        private Enemy _currentEnemy;
        private float _currentMaxHealth;

        private void OnEnable()
        {
            EnemySpawnedSignal.OnEnemySpawned += OnEnemySpawned;
            EnemyHitSignal.OnEnemyHit += OnEnemyHit;
            EnemyKilledSignal.OnEnemyKilled += OnEnemyKilled;
        }

        private void OnDisable()
        {
            EnemySpawnedSignal.OnEnemySpawned -= OnEnemySpawned;
            EnemyHitSignal.OnEnemyHit -= OnEnemyHit;
            EnemyKilledSignal.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemySpawned(EnemySpawnedInfo info)
        {
            _currentEnemy = info.Enemy;
            _currentMaxHealth = info.Enemy.MaxHealth;
        }

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            // The enemy object is destroyed right after the signal; drop the cache to avoid NRE
            _currentEnemy = null;
            _currentMaxHealth = 0f;
        }

        private void OnEnemyHit(EnemyHitEvent evt)
        {
            var enemy = _currentEnemy;
            if (enemy == null || _canvas == null)
            {
                return;
            }

            // Restore the absolute damage from the percent against the cached max health
            var damage = MathfRound.Round(evt.DamagePercent * _currentMaxHealth);
            if (damage < MinDamageToShow)
            {
                return;
            }

            var camera = Camera.main;
            if (camera == null)
            {
                return;
            }

            var spawnPoint = camera.WorldToScreenPoint(GetPopupWorldPosition(enemy));
            if (spawnPoint.z < 0f)
            {
                return;
            }

            SpawnPopup(spawnPoint, damage, evt.DamagePercent >= CriticalDamagePercent);
        }

        private static Vector3 GetPopupWorldPosition(Enemy enemy)
        {
            var position = enemy.transform.position;

            if (enemy.TryGetComponent<SpriteRenderer>(out var spriteRenderer))
            {
                position.y += spriteRenderer.bounds.extents.y;
            }

            return position;
        }

        private void SpawnPopup(Vector3 screenPoint, float damage, bool isCritical)
        {
            var popup = new GameObject("DamagePopup", typeof(RectTransform), typeof(CanvasGroup));
            var rect = (RectTransform)popup.transform;
            rect.SetParent(_canvas.transform, false);

            // Anchor to the canvas bottom-left; screen pixels convert to canvas units via scaleFactor
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.zero;
            rect.pivot = new Vector2(0.5f, 0.5f);
            var point = (Vector2)screenPoint / _canvas.scaleFactor;
            rect.anchoredPosition = new Vector2(
                point.x + Random.Range(-_xSpread, _xSpread),
                point.y + _yOffset);

            var text = popup.AddComponent<TextMeshProUGUI>();
            text.text = string.Format(TextFormat, Mathf.Max(1, Mathf.FloorToInt(damage)));
            text.fontSize = _fontSize;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;
            text.color = isCritical ? CriticalColor : NormalColor;

            var sequence = DOTween.Sequence();
            sequence.Append(rect.DOAnchorPosY(rect.anchoredPosition.y + _riseDistance, _lifetime)
                .SetEase(Ease.OutQuad));
            sequence.Join(popup.GetComponent<CanvasGroup>().DOFade(0f, _lifetime));
            sequence.OnComplete(() => Destroy(popup));
        }
    }
}
