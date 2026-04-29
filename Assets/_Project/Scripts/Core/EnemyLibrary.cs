using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

namespace _Project.Scripts.Core
{
    [CreateAssetMenu(menuName = "EnemyLibrary")]
    public class EnemyLibrary : ScriptableObject
    {
        [Header("Datas")] [SerializeField] private EnemyLibraryData _enemyLibraryData;

        [SerializeField] private BossLibraryData _bossLibraryData;

        [Header("Prefabs")] [SerializeField] private List<Enemy> _enemiesPrefab;

        [SerializeField] private List<Enemy> _bossesPrefabs;

        public float EnemyReward => _enemyLibraryData.DefaultReward;
        public float BossReward => _bossLibraryData.DefaultReward;

        public Enemy GetRandomEnemyPrefab(int seed)
        {
            if (_enemiesPrefab == null || _enemiesPrefab.Count == 0)
            {
                Debug.LogError("EnemyLibrary: нет префабов обычных мобов!");
                return null;
            }

            Random.InitState(seed);
            var index = Random.Range(0, _enemiesPrefab.Count);
            return _enemiesPrefab[index];
        }

        public Enemy GetRandomBossPrefab(int seed)
        {
            if (_bossesPrefabs == null || _bossesPrefabs.Count == 0)
            {
                Debug.LogError("EnemyLibrary: нет префабов боссов!");
                return null;
            }

            Random.InitState(seed);
            var index = Random.Range(0, _bossesPrefabs.Count);
            return _bossesPrefabs[index];
        }

        public float CalculateHealth(int level)
        {
            return _enemyLibraryData.DefaultHealth * Mathf.Pow(1.15f, level - 1);
        }

        public int CalculateBossHealth(int level)
        {
            return Mathf.RoundToInt(_bossLibraryData.DefaultHealth * Mathf.Pow(1.15f, level - 1));
        }
    }
}