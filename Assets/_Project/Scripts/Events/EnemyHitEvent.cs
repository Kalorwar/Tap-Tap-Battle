using _Project.Scripts.Enemies;
using R3;

namespace _Project.Scripts.Events
{
    public static class EnemyHitSignal
    {
        private static readonly Subject<EnemyHitEvent> OnEnemyHitSubject = new();
        public static Observable<EnemyHitEvent> OnEnemyHit => OnEnemyHitSubject;

        public static void Emit(EnemyHitEvent evt)
        {
            OnEnemyHitSubject.OnNext(evt);
        }
    }
}