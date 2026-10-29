using System;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class RewardService : IDisposable
    {
        private const float BossRewardMultiplier = 5f;

        private readonly ICurrencyController _currencyController;
        private readonly EnemyLibrary _enemyLibrary;

        public RewardService(EnemyLibrary enemyLibrary, ICurrencyController currencyController)
        {
            _enemyLibrary = enemyLibrary;
            _currencyController = currencyController;

            EnemyKilledSignal.OnEnemyKilled += OnEnemyKilled;
        }

        public void Dispose()
        {
            EnemyKilledSignal.OnEnemyKilled -= OnEnemyKilled;
        }

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            var softReward = CalculateSoftReward(info.Level, info.IsBoss);
            ApplyReward(softReward, CurrencyType.SoftCurrency);

            if (info.IsBoss)
            {
                ApplyReward(_enemyLibrary.BossReward, CurrencyType.HardCurrency);
            }
        }

        private float CalculateSoftReward(int level, bool isBoss)
        {
            var reward = MathfRound.Round(_enemyLibrary.EnemyReward * Mathf.Pow(1.12f, level - 1));
            return isBoss ? reward * BossRewardMultiplier : reward;
        }

        private void ApplyReward(float reward, CurrencyType currencyType)
        {
            _currencyController.AddCurrency(reward, currencyType);
        }
    }
}
