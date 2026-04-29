using R3;

namespace _Project.Scripts.Core
{
    public static class EnemyDiedEvent
    {
        private static readonly Subject<Unit> OnEnemyDeadSubject = new();
        public static Observable<Unit> OnEnemyDead => OnEnemyDeadSubject;

        public static void Emit()
        {
            OnEnemyDeadSubject.OnNext(Unit.Default);
        }
    }
}