using System;
using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Assistants
{
    public class AssistantService : IDisposable
    {
        private const string LevelKeyFormat = "Assistants.{0}.Level";

        private readonly IReadOnlyList<AssistantConfig> _configs;
        private readonly ICurrencyController _currency;
        private readonly LevelCountService _levelCount;

        private int _bossesDefeated;

        public AssistantService(IReadOnlyList<AssistantConfig> configs, ICurrencyController currency,
            LevelCountService levelCount)
        {
            _configs = configs;
            _currency = currency;
            _levelCount = levelCount;
            _bossesDefeated = levelCount.BossesDefeated;

            BossDefeatedSignal.OnBossDefeated += OnBossDefeated;
            LevelCompletedSignal.OnLevelCompleted += OnLevelCompleted;
        }

        public void Dispose()
        {
            BossDefeatedSignal.OnBossDefeated -= OnBossDefeated;
            LevelCompletedSignal.OnLevelCompleted -= OnLevelCompleted;
        }

        public float TotalDps
        {
            get
            {
                var total = 0f;

                foreach (var config in _configs)
                {
                    total += GetDps(config);
                }

                return total;
            }
        }

        public int GetLevel(string id)
        {
            return PlayerPrefs.GetInt(GetLevelKey(id), 0);
        }

        public bool IsUnlocked(AssistantConfig config)
        {
            if (config.UnlockLevel > 0 && _levelCount.LevelCount < config.UnlockLevel)
            {
                return false;
            }

            if (config.UnlockBossIndex > 0 && _bossesDefeated < config.UnlockBossIndex)
            {
                return false;
            }

            return true;
        }

        public float GetDps(AssistantConfig config)
        {
            var level = GetLevel(config.Id);
            if (level < 1)
            {
                return 0f;
            }

            return config.BaseDps * Mathf.Pow(config.DpsGrowth, level - 1);
        }

        public float GetCost(AssistantConfig config)
        {
            var level = GetLevel(config.Id);
            return config.BaseCost * Mathf.Pow(config.CostGrowth, level);
        }

        public bool TryBuy(AssistantConfig config)
        {
            if (!IsUnlocked(config))
            {
                return false;
            }

            var cost = GetCost(config);
            if (!_currency.TryRemoveCurrency(cost, CurrencyType.SoftCurrency))
            {
                return false;
            }

            var newLevel = GetLevel(config.Id) + 1;
            PlayerPrefs.SetInt(GetLevelKey(config.Id), newLevel);
            PlayerPrefs.Save();

            AssistantChangedSignal.Emit();
            return true;
        }

        private static string GetLevelKey(string id)
        {
            return string.Format(LevelKeyFormat, id);
        }

        private void OnBossDefeated(int bossIndex)
        {
            _bossesDefeated = Mathf.Max(_bossesDefeated, bossIndex);

            foreach (var config in _configs)
            {
                if (config.UnlockBossIndex == _bossesDefeated)
                {
                    AssistantChangedSignal.Emit();
                    return;
                }
            }
        }

        private void OnLevelCompleted(int level)
        {
            foreach (var config in _configs)
            {
                if (config.UnlockLevel == level)
                {
                    AssistantChangedSignal.Emit();
                    return;
                }
            }
        }
    }
}
