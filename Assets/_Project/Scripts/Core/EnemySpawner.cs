using System;
using _Project.Scripts.Events;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Core
{
    public class EnemySpawner : MonoBehaviour
    {
        [SerializeField] private Transform _spawnPoint;
        private DamageController _damageController;

        private EnemyLibrary _enemyLibrary;
        private LevelCountService _levelCountService;
        private SeedService _seedService;

        [Inject]
        private void Construct(EnemyLibrary enemyLibrary, LevelCountService levelCountService,
            DamageController damageController)
        {
            _enemyLibrary = enemyLibrary;
            _levelCountService = levelCountService;
            _damageController = damageController;
        }

        private void Awake()
        {
            _seedService = new SeedService();
        }

        private void Start()
        {
            Spawn();
        }

        private void OnEnable()
        {
            LevelCompletedSignal.OnLevelCompleted += OnLevelCompleted;
        }

        private void OnDisable()
        {
            LevelCompletedSignal.OnLevelCompleted -= OnLevelCompleted;
            _seedService?.Dispose();
        }

        private void OnLevelCompleted(int level)
        {
            Spawn();
        }

        private void Spawn()
        {
            var prefab = _enemyLibrary.GetRandomEnemyPrefab(_seedService.CurrentSeed);
            if (prefab == null)
            {
                return;
            }

            var enemy = Instantiate(prefab, _spawnPoint.position, Quaternion.identity, _spawnPoint);

            var health = _enemyLibrary.CalculateHealth(_levelCountService.LevelCount);

            enemy.Configure(health);
            _damageController.SetTarget(enemy);
            EnemySpawnedSignal.Emit(new EnemySpawnedInfo(enemy, false));
        }
    }
}
