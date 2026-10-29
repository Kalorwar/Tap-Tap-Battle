using System;
using _Project.Scripts.Achievements;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised exactly once per achievement when its goal is reached.
    /// Payload: basic info about the unlocked achievement.
    /// </summary>
    public static class AchievementUnlockedSignal
    {
        public static event Action<AchievementInfo> OnAchievementUnlocked;

        public static void Emit(AchievementInfo info)
        {
            OnAchievementUnlocked?.Invoke(info);
        }
    }
}
