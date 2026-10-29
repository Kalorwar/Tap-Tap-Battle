using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised when assistants state changes (purchase, unlock).
    /// </summary>
    public static class AssistantChangedSignal
    {
        public static event Action OnAssistantChanged;

        public static void Emit()
        {
            OnAssistantChanged?.Invoke();
        }
    }
}
