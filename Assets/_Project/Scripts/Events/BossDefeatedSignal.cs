using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised once per successfully defeated boss (boss levels are every 10th level).
    /// Payload: 1-based boss index (first defeated boss = 1).
    /// Used by assistant unlocks and boss counters.
    /// </summary>
    public static class BossDefeatedSignal
    {
        public static event Action<int> OnBossDefeated;

        public static void Emit(int bossIndex)
        {
            OnBossDefeated?.Invoke(bossIndex);
        }
    }
}
