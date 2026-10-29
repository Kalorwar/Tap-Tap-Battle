using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised whenever achievement progress or unlock state changed; UI should refresh.
    /// </summary>
    public static class AchievementsChangedSignal
    {
        public static event Action OnAchievementsChanged;

        public static void Emit()
        {
            OnAchievementsChanged?.Invoke();
        }
    }
}
