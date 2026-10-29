using System;
using _Project.Scripts.Enemies;

namespace _Project.Scripts.Events
{
    public static class EnemyHitSignal
    {
        public static event Action<EnemyHitEvent> OnEnemyHit;

        public static void Emit(EnemyHitEvent evt)
        {
            OnEnemyHit?.Invoke(evt);
        }
    }
}
