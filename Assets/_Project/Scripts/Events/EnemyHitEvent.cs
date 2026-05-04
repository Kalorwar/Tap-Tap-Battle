using _Project.Scripts.Enemies;
using R3;

namespace _Project.Scripts.Events
{
    public static class EnemyHitSignal
    {
        private static readonly Subject<EnemyHitEvent> _onEnemyHit = new();
        public static Observable<EnemyHitEvent> OnEnemyHit => _onEnemyHit;

        public static void Emit(EnemyHitEvent evt)
        {
            _onEnemyHit.OnNext(evt);
        }
    }
}