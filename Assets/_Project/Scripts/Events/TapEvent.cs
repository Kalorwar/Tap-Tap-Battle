using R3;

namespace _Project.Scripts.Events
{
    public static class TapEvent
    {
        private static readonly Subject<Unit> OnTappedSubject = new();
        public static Observable<Unit> OnTapped => OnTappedSubject;

        public static void Emit()
        {
            OnTappedSubject.OnNext(Unit.Default);
        }
    }
}