using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised when daily quests change: new window generated, quest completed, quest claimed.
    /// </summary>
    public static class QuestsChangedSignal
    {
        public static event Action OnQuestsChanged;

        public static void Emit()
        {
            OnQuestsChanged?.Invoke();
        }
    }
}
