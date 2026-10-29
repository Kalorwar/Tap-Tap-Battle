using System;
using System.Collections.Generic;
using System.Globalization;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using _Project.Scripts.Upgrades;
using UnityEngine;

namespace _Project.Scripts.Achievements
{
    /// <summary>
    /// Persistent achievement system: tracks kill/boss/tap-damage counters and the coin balance,
    /// unlocks achievements exactly once and pays diamond rewards via AchievementUnlockedSignal.
    /// Lives forever as a Zenject single, so static event subscriptions are never unsubscribed.
    /// </summary>
    public class AchievementService
    {
        private const string TotalKillsKey = "Achievements.TotalKills";
        private const string TotalBossesKey = "Achievements.TotalBosses";
        private const string TotalTapDamageKey = "Achievements.TotalTapDamage";
        private const string UnlockedKeyFormat = "Achievements.Unlocked.{0}";

        private static readonly AchievementDef[] Definitions =
        {
            new AchievementDef("novice", "Новичок", "Убей 100 врагов", 100f, 10, AchievementSource.Kills),
            new AchievementDef("finisher", "Добиватель", "Убей 10 000 врагов", 10000f, 50, AchievementSource.Kills),
            new AchievementDef("slayer", "Slayer", "Победи 50 боссов", 50f, 100, AchievementSource.Bosses),
            new AchievementDef("millionaire", "Миллионер", "Накопи 1 000 000 монет", 1000000f, 25, AchievementSource.Coins),
            new AchievementDef("tapmaster", "Безрукий", "Нанеси 1 000 000 урона тапами", 1000000f, 50, AchievementSource.TapDamage)
        };

        private readonly ICurrencyController _currencyController;
        private readonly TapDamageUpgradeService _tapDamageUpgradeService;

        private readonly HashSet<string> _unlockedIds = new HashSet<string>();

        private long _totalKills;
        private long _totalBosses;
        private double _totalTapDamage;

        public AchievementService(ICurrencyController currencyController,
            TapDamageUpgradeService tapDamageUpgradeService)
        {
            _currencyController = currencyController;
            _tapDamageUpgradeService = tapDamageUpgradeService;

            Load();

            EnemyKilledSignal.OnEnemyKilled += OnEnemyKilled;
            BossDefeatedSignal.OnBossDefeated += OnBossDefeated;
            TapEvent.OnTapped += OnTapped;
            _currencyController.OnCurrencyChange += OnCurrencyChange;

            // Covers millionaire on start and goals reached between sessions
            CheckAll();
        }

        public IReadOnlyList<AchievementEntry> Entries
        {
            get
            {
                var entries = new List<AchievementEntry>(Definitions.Length);
                foreach (var def in Definitions)
                {
                    entries.Add(new AchievementEntry(def.Id, def.DisplayName, def.GoalText, def.Goal,
                        GetProgress(def), _unlockedIds.Contains(def.Id), def.DiamondReward));
                }

                return entries;
            }
        }

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            _totalKills++;
            SaveCounters();
            OnProgressChanged(AchievementSource.Kills);
        }

        private void OnBossDefeated(int bossIndex)
        {
            // Payload is the 1-based cumulative boss count; keep the maximum to stay idempotent
            var bosses = Math.Max(_totalBosses, bossIndex);
            if (bosses == _totalBosses)
            {
                return;
            }

            _totalBosses = bosses;
            SaveCounters();
            OnProgressChanged(AchievementSource.Bosses);
        }

        private void OnTapped()
        {
            _totalTapDamage += _tapDamageUpgradeService.CurrentDamage;
            SaveCounters();
            OnProgressChanged(AchievementSource.TapDamage);
        }

        private void OnCurrencyChange()
        {
            OnProgressChanged(AchievementSource.Coins);
        }

        private void OnProgressChanged(AchievementSource source)
        {
            AchievementsChangedSignal.Emit();
            CheckSource(source);
        }

        private void CheckAll()
        {
            CheckSource(AchievementSource.Kills);
            CheckSource(AchievementSource.Bosses);
            CheckSource(AchievementSource.Coins);
            CheckSource(AchievementSource.TapDamage);
        }

        private void CheckSource(AchievementSource source)
        {
            foreach (var def in Definitions)
            {
                if (def.Source == source)
                {
                    TryUnlock(def);
                }
            }
        }

        private void TryUnlock(AchievementDef def)
        {
            if (_unlockedIds.Contains(def.Id))
            {
                return;
            }

            if (GetProgress(def) < def.Goal)
            {
                return;
            }

            // Flag before paying the reward: OnCurrencyChange re-enters checks
            _unlockedIds.Add(def.Id);
            PlayerPrefs.SetInt(string.Format(UnlockedKeyFormat, def.Id), 1);
            PlayerPrefs.Save();

            _currencyController.AddCurrency(def.DiamondReward, CurrencyType.HardCurrency);
            AchievementsChangedSignal.Emit();
            AchievementUnlockedSignal.Emit(new AchievementInfo(def.Id, def.DisplayName, def.DiamondReward));
        }

        private float GetProgress(AchievementDef def)
        {
            switch (def.Source)
            {
                case AchievementSource.Kills:
                    return _totalKills;
                case AchievementSource.Bosses:
                    return _totalBosses;
                case AchievementSource.Coins:
                    return _currencyController.GetAmount(CurrencyType.SoftCurrency);
                case AchievementSource.TapDamage:
                    return (float)_totalTapDamage;
                default:
                    return 0f;
            }
        }

        private void Load()
        {
            _totalKills = ParseLong(PlayerPrefs.GetString(TotalKillsKey, "0"));
            _totalBosses = ParseLong(PlayerPrefs.GetString(TotalBossesKey, "0"));
            _totalTapDamage = ParseDouble(PlayerPrefs.GetString(TotalTapDamageKey, "0"));

            foreach (var def in Definitions)
            {
                var unlockedKey = string.Format(UnlockedKeyFormat, def.Id);
                if (PlayerPrefs.GetInt(unlockedKey, 0) == 1)
                {
                    _unlockedIds.Add(def.Id);
                }
            }
        }

        private void SaveCounters()
        {
            PlayerPrefs.SetString(TotalKillsKey, _totalKills.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(TotalBossesKey, _totalBosses.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.SetString(TotalTapDamageKey, _totalTapDamage.ToString(CultureInfo.InvariantCulture));
            PlayerPrefs.Save();
        }

        private static long ParseLong(string value)
        {
            return long.TryParse(value, NumberStyles.Integer, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0L;
        }

        private static double ParseDouble(string value)
        {
            return double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out var result)
                ? result
                : 0d;
        }

        private enum AchievementSource
        {
            Kills,
            Bosses,
            Coins,
            TapDamage
        }

        private readonly struct AchievementDef
        {
            public readonly string Id;
            public readonly string DisplayName;
            public readonly string GoalText;
            public readonly float Goal;
            public readonly int DiamondReward;
            public readonly AchievementSource Source;

            public AchievementDef(string id, string displayName, string goalText, float goal, int diamondReward,
                AchievementSource source)
            {
                Id = id;
                DisplayName = displayName;
                GoalText = goalText;
                Goal = goal;
                DiamondReward = diamondReward;
                Source = source;
            }
        }
    }

    /// <summary>
    /// Snapshot of one achievement for the UI.
    /// </summary>
    public readonly struct AchievementEntry
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly string GoalText;
        public readonly float Goal;
        public readonly float Progress;
        public readonly bool IsUnlocked;
        public readonly int DiamondReward;

        public AchievementEntry(string id, string displayName, string goalText, float goal, float progress,
            bool isUnlocked, int diamondReward)
        {
            Id = id;
            DisplayName = displayName;
            GoalText = goalText;
            Goal = goal;
            Progress = progress;
            IsUnlocked = isUnlocked;
            DiamondReward = diamondReward;
        }
    }
}
