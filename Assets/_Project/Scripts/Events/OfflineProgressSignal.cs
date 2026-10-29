using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised after pending offline coins were claimed; UI should refresh.
    /// </summary>
    public static class OfflineProgressSignal
    {
        public static event Action OnOfflineProgress;

        public static void Emit()
        {
            OnOfflineProgress?.Invoke();
        }
    }
}
