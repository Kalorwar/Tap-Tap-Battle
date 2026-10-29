using System;
using _Project.Scripts.CurrencyController;

namespace _Project.Scripts.Events
{
    /// <summary>
    /// Raised after a daily reward has been claimed. Payload: claimed reward info.
    /// </summary>
    public static class DailyRewardClaimedSignal
    {
        public static event Action<DailyRewardInfo> OnDailyRewardClaimed;

        public static void Emit(DailyRewardInfo info)
        {
            OnDailyRewardClaimed?.Invoke(info);
        }
    }

    public readonly struct DailyRewardInfo
    {
        public readonly int Day;
        public readonly CurrencyType RewardType;
        public readonly float RewardAmount;
        public readonly int Streak;

        public DailyRewardInfo(int day, CurrencyType rewardType, float rewardAmount, int streak)
        {
            Day = day;
            RewardType = rewardType;
            RewardAmount = rewardAmount;
            Streak = streak;
        }
    }
}
