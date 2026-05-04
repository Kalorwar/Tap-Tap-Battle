using System;
using R3;
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
        private IDisposable _subscription;

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
            _subscription = LevelCompletedSignal.OnLevelCompleted.Subscribe(_ => Spawn());
        }

        private void OnDisable()
        {
            _subscription?.Dispose();
            _seedService?.Dispose();
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
        }
    }
}