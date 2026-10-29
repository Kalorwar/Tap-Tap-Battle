using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised every time an enemy (normal or boss) is killed by the player.
    /// Payload: whether the killed enemy was a boss and the level it died on.
    /// </summary>
    public static class EnemyKilledSignal
    {
        public static event Action<EnemyKilledInfo> OnEnemyKilled;

        public static void Emit(EnemyKilledInfo info)
        {
            OnEnemyKilled?.Invoke(info);
        }
    }

    public readonly struct EnemyKilledInfo
    {
        public readonly bool IsBoss;
        public readonly int Level;

        public EnemyKilledInfo(bool isBoss, int level)
        {
            IsBoss = isBoss;
            Level = level;
        }
    }
}
