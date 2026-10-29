using _Project.Scripts.Core;
using _Project.Scripts.Enemies;
using _Project.Scripts.Events;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Assistants
{
    public class DpsTicker : MonoBehaviour
    {
        private const float TickInterval = 1f;

        private AssistantService _assistantService;
        private Enemy _currentEnemy;
        private float _timer;

        [Inject]
        private void Construct(AssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        private void Update()
        {
            _timer += Time.deltaTime;

            while (_timer >= TickInterval)
            {
                _timer -= TickInterval;
                ApplyDps();
            }
        }

        private void ApplyDps()
        {
            var totalDps = _assistantService.TotalDps;
            if (totalDps <= 0f)
            {
                return;
            }

            if (_currentEnemy == null || !_currentEnemy.IsAlive)
            {
                return;
            }

            _currentEnemy.TakeDamage(MathfRound.Round(totalDps));
        }

        private void OnEnable()
        {
            EnemySpawnedSignal.OnEnemySpawned += OnEnemySpawned;
            EnemyDiedEvent.OnEnemyDead += OnEnemyDead;
        }

        private void OnDisable()
        {
            EnemySpawnedSignal.OnEnemySpawned -= OnEnemySpawned;
            EnemyDiedEvent.OnEnemyDead -= OnEnemyDead;
        }

        private void OnEnemySpawned(EnemySpawnedInfo info)
        {
            _currentEnemy = info.Enemy;
        }

        private void OnEnemyDead()
        {
            _currentEnemy = null;
        }
    }
}
