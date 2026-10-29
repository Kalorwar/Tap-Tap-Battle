using System;

namespace _Project.Scripts.Events
{
    public static class TapEvent
    {
        public static event Action OnTapped;

        public static void Emit()
        {
            OnTapped?.Invoke();
        }
    }
}
