using System;
using _Project.Scripts.Enemies;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised every time a new enemy (normal or boss) is spawned and configured.
    /// Payload: the enemy instance and whether it is a boss fight.
    /// </summary>
    public static class EnemySpawnedSignal
    {
        public static event Action<EnemySpawnedInfo> OnEnemySpawned;

        public static void Emit(EnemySpawnedInfo info)
        {
            OnEnemySpawned?.Invoke(info);
        }
    }

    public readonly struct EnemySpawnedInfo
    {
        public readonly Enemy Enemy;
        public readonly bool IsBoss;

        public EnemySpawnedInfo(Enemy enemy, bool isBoss)
        {
            Enemy = enemy;
            IsBoss = isBoss;
        }
    }
}
