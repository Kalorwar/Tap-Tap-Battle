using System;
using _Project.Scripts.Enemies;
using _Project.Scripts.Events;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Core
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;

        private DamageController _damageController;
        private BossFightService _bossFightService;
        private EnemyLibrary _enemyLibrary;
        private LevelCountService _levelCountService;
        private SeedService _seedService;

        private Enemy _currentEnemy;
        private bool _currentEnemyIsBoss;

        [Inject]
        private void Construct(EnemyLibrary enemyLibrary, LevelCountService levelCountService,
            DamageController damageController, BossFightService bossFightService)
        {
            _enemyLibrary = enemyLibrary;
            _levelCountService = levelCountService;
            _damageController = damageController;
            _bossFightService = bossFightService;
        }

        private void Awake()
        {
            _seedService = new SeedService();
        }

        private void Start()
        {
            Spawn();
        }

        private void Update()
        {
            _bossFightService.Tick(Time.deltaTime);
        }

        private void OnEnable()
        {
            LevelCompletedSignal.OnLevelCompleted += OnLevelCompleted;
            EnemyDiedEvent.OnEnemyDead += OnEnemyDead;
            _bossFightService.BossTimedOut += OnBossTimedOut;
        }

        private void OnDisable()
        {
            LevelCompletedSignal.OnLevelCompleted -= OnLevelCompleted;
            EnemyDiedEvent.OnEnemyDead -= OnEnemyDead;
            _bossFightService.BossTimedOut -= OnBossTimedOut;
            _seedService?.Dispose();
        }

        private void OnLevelCompleted(int level)
        {
            Spawn();
        }

        private void OnEnemyDead()
        {
            var wasBoss = _currentEnemyIsBoss;
            EnemyKilledSignal.Emit(new EnemyKilledInfo(wasBoss, _levelCountService.LevelCount));

            // Completed levels spawn through LevelCompletedSignal, otherwise the next enemy of the same level
            if (!wasBoss && _levelCountService.KillsOnLevel > 0)
            {
                SpawnEnemy();
            }
        }

        private void OnBossTimedOut()
        {
            if (_currentEnemy != null)
            {
                Destroy(_currentEnemy.gameObject);
            }

            // Boss respawns with full HP and the timer starts over; level stays the same
            Spawn();
        }

        private void Spawn()
        {
            if (_levelCountService.IsBossLevel)
            {
                SpawnBoss();
            }
            else
            {
                SpawnEnemy();
            }
        }

        private void SpawnEnemy()
        {
            var prefab = _enemyLibrary.GetRandomEnemyPrefab(_seedService.CurrentSeed);
            SpawnPrefab(prefab, _enemyLibrary.CalculateHealth(_levelCountService.LevelCount), false);
        }

        private void SpawnBoss()
        {
            var prefab = _enemyLibrary.GetRandomBossPrefab(_seedService.CurrentSeed);
            SpawnPrefab(prefab, _enemyLibrary.CalculateBossHealth(_levelCountService.LevelCount), true);
        }

        private void SpawnPrefab(Enemy prefab, float health, bool isBoss)
        {
            if (prefab == null)
            {
                return;
            }

            var enemy = Instantiate(prefab, _spawnPoint.position, Quaternion.identity, _spawnPoint);

            _currentEnemy = enemy;
            _currentEnemyIsBoss = isBoss;

            enemy.Configure(health);
            _damageController.SetTarget(enemy);
            EnemySpawnedSignal.Emit(new EnemySpawnedInfo(enemy, isBoss));
        }
    }
}
