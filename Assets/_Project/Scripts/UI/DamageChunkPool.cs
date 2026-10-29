using System;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Pool;
using UnityEngine.UI;
using Object = UnityEngine.Object;

namespace _Project.Scripts.UI
{
    public sealed class DamageChunkPool : IDisposable
    {
        private readonly Color _baseColor;
        private readonly Transform _container;
        private readonly ObjectPool<Image> _pool;
        private readonly Image _prefab;

        public DamageChunkPool(Image prefab, Transform container, Color baseColor, int defaultSize = 5,
            int maxSize = 20)
        {
            _prefab = prefab;
            _container = container;
            _baseColor = baseColor;

            _pool = new ObjectPool<Image>(
                CreateChunk,
                OnGet,
                OnRelease,
                OnDestroyChunk,
                false,
                defaultSize,
                maxSize
            );
        }

        public void Dispose()
        {
            _pool.Dispose();
        }

        public Image Get()
        {
            return _pool.Get();
        }

        public void Release(Image chunk)
        {
            _pool.Release(chunk);
        }

        private Image CreateChunk()
        {
            return Object.Instantiate(_prefab, _container);
        }

        private void OnGet(Image chunk)
        {
            chunk.gameObject.SetActive(true);
            chunk.color = new Color(
                _baseColor.r * 0.8f,
                _baseColor.g * 0.8f,
                _baseColor.b * 0.8f,
                0.6f
            );
        }

        private void OnRelease(Image chunk)
        {
            DOTween.Kill(chunk.rectTransform);
            DOTween.Kill(chunk);
            chunk.gameObject.SetActive(false);
        }

        private void OnDestroyChunk(Image chunk)
        {
            Object.Destroy(chunk.gameObject);
        }
    }
}