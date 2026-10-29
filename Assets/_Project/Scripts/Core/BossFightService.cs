using System;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Core
{
    // Boss fight timer; ticking is driven externally via Tick(deltaTime)
    public class BossFightService : IDisposable
    {
        private const float BossFightDurationSeconds = 30f;

        private float _remainingSeconds;
        private float _lastEmittedSecond;
        private bool _isRunning;

        public BossFightService()
        {
            EnemySpawnedSignal.OnEnemySpawned += OnEnemySpawned;
            BossDefeatedSignal.OnBossDefeated += OnBossDefeated;
        }

        public event Action BossTimedOut;

        public bool IsRunning => _isRunning;

        public void Dispose()
        {
            EnemySpawnedSignal.OnEnemySpawned -= OnEnemySpawned;
            BossDefeatedSignal.OnBossDefeated -= OnBossDefeated;
        }

        public void Tick(float deltaTime)
        {
            if (!_isRunning)
            {
                return;
            }

            _remainingSeconds -= deltaTime;
            if (_remainingSeconds > 0f)
            {
                EmitSecondChanged();
                return;
            }

            _remainingSeconds = 0f;
            Stop();
            BossTimedOut?.Invoke();
        }

        private void OnEnemySpawned(EnemySpawnedInfo info)
        {
            if (info.IsBoss)
            {
                Start();
            }
        }

        private void OnBossDefeated(int bossIndex)
        {
            Stop();
        }

        private void Start()
        {
            _remainingSeconds = BossFightDurationSeconds;
            _isRunning = true;
            _lastEmittedSecond = BossFightDurationSeconds;
            Emit();
        }

        private void Stop()
        {
            if (!_isRunning)
            {
                return;
            }

            _isRunning = false;
            Emit();
        }

        private void EmitSecondChanged()
        {
            var second = Mathf.CeilToInt(_remainingSeconds);
            if (Mathf.Approximately(second, _lastEmittedSecond))
            {
                return;
            }

            _lastEmittedSecond = second;
            Emit();
        }

        private void Emit()
        {
            BossTimerSignal.Emit(new BossTimerInfo(_isRunning, Mathf.CeilToInt(_remainingSeconds)));
        }
    }
}
