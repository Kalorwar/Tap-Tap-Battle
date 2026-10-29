using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised when the daily rewards state changes (claim, streak reset, service start).
    /// </summary>
    public static class DailyChangedSignal
    {
        public static event Action OnDailyChanged;

        public static void Emit()
        {
            OnDailyChanged?.Invoke();
        }
    }
}
