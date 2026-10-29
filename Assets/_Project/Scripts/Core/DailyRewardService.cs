using System;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class DailyRewardService
    {
        private const string LastClaimKey = "Daily.LastClaimUtc";
        private const string StreakKey = "Daily.Streak";

        private const int CycleLength = 7;
        private const float ClaimCooldownHours = 20f;
        private const float StreakResetHours = 48f;

        // 7-day reward cycle from the GDD (index = day - 1)
        private static readonly DayReward[] Rewards =
        {
            new DayReward(CurrencyType.SoftCurrency, 50f),
            new DayReward(CurrencyType.SoftCurrency, 100f),
            new DayReward(CurrencyType.HardCurrency, 5f),
            new DayReward(CurrencyType.SoftCurrency, 500f),
            new DayReward(CurrencyType.HardCurrency, 10f),
            new DayReward(CurrencyType.SoftCurrency, 1000f),
            new DayReward(CurrencyType.HardCurrency, 25f),
        };

        private readonly ICurrencyController _currencyController;

        private DateTime _lastClaimUtc;
        private int _streak;

        public DailyRewardService(ICurrencyController currencyController)
        {
            _currencyController = currencyController;

            _lastClaimUtc = DateTime.FromBinary(PlayerPrefsLong.Get(LastClaimKey, 0L));
            _streak = PlayerPrefs.GetInt(StreakKey, 0);
            RefreshStreak();

            DailyChangedSignal.Emit();
        }

        public bool CanClaim => ElapsedHours >= ClaimCooldownHours;

        public int CurrentStreak => _streak;

        // Day of the cycle to claim next (1..7); streak keeps growing until it breaks
        public int NextDay => _streak % CycleLength + 1;

        public void Claim()
        {
            if (!CanClaim)
            {
                return;
            }

            var day = NextDay;
            var reward = Rewards[day - 1];

            _currencyController.AddCurrency(reward.Amount, reward.Type);

            _streak++;
            _lastClaimUtc = DateTime.UtcNow;
            Save();

            DailyRewardClaimedSignal.Emit(new DailyRewardInfo(day, reward.Type, reward.Amount, _streak));
            DailyChangedSignal.Emit();
        }

        public string DescribeDay(int day1to7)
        {
            var day = Mathf.Clamp(day1to7, 1, CycleLength);
            var reward = Rewards[day - 1];

            return reward.Type == CurrencyType.SoftCurrency
                ? $"{reward.Amount:0} монет"
                : $"{reward.Amount:0} алмазов";
        }

        // Streak breaks when more than 48h passed since the last claim
        private void RefreshStreak()
        {
            if (_streak == 0 || ElapsedHours <= StreakResetHours)
            {
                return;
            }

            _streak = 0;
            Save();
        }

        private double ElapsedHours => (DateTime.UtcNow - _lastClaimUtc).TotalHours;

        private void Save()
        {
            PlayerPrefsLong.Set(LastClaimKey, _lastClaimUtc.ToBinary());
            PlayerPrefs.SetInt(StreakKey, _streak);
            PlayerPrefs.Save();
        }

        private readonly struct DayReward
        {
            public readonly CurrencyType Type;
            public readonly float Amount;

            public DayReward(CurrencyType type, float amount)
            {
                Type = type;
                Amount = amount;
            }
        }
    }
}
