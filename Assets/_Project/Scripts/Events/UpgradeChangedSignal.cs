using System;

namespace _Project.Scripts.Events
{
    public static class UpgradeChangedSignal
    {
        public static event Action OnUpgradeChanged;

        public static void Emit()
        {
            OnUpgradeChanged?.Invoke();
        }
    }
}
