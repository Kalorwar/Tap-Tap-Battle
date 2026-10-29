using System;

namespace _Project.Scripts.Events
{
    public static class EnemyDiedEvent
    {
        public static event Action OnEnemyDead;

        public static void Emit()
        {
            OnEnemyDead?.Invoke();
        }
    }
}
