using System;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised when the boss fight timer starts, on every elapsed second and when it stops.
    /// Payload: whether the timer is running and the remaining seconds.
    /// </summary>
    public static class BossTimerSignal
    {
        public static event Action<BossTimerInfo> OnBossTimer;

        public static void Emit(BossTimerInfo info)
        {
            OnBossTimer?.Invoke(info);
        }
    }

    public readonly struct BossTimerInfo
    {
        public readonly bool IsRunning;
        public readonly float RemainingSeconds;

        public BossTimerInfo(bool isRunning, float remainingSeconds)
        {
            IsRunning = isRunning;
            RemainingSeconds = remainingSeconds;
        }
    }
}
