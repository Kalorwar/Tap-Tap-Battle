using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public static class DamageChunkAnimator
    {
        private static readonly Color WhiteTranslucent = new(1f, 1f, 1f, 0.9f);

        public static void PlayChunkAnimation(
            Image chunk,
            RectTransform barRect,
            float fromPercent,
            float toPercent,
            DamageChunkAnimationSettings settings,
            DamageChunkPool pool)
        {
            var chunkRect = chunk.rectTransform;
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

            var seq = DOTween.Sequence();

            // 1. Flash to white
            seq.Append(chunk.DOColor(WhiteTranslucent, settings.FlashDuration)
                .SetEase(Ease.OutQuad));

            // 2. Fade out + move down
            seq.Append(chunk.DOFade(0f, settings.FadeDuration));
            seq.Join(chunkRect
                .DOAnchorPosY(
                    chunkRect.anchoredPosition.y - settings.MoveDistance,
                    settings.FadeDuration)
                .SetEase(Ease.OutQuad));

            seq.OnComplete(() => pool.Release(chunk));
        }
    }
}