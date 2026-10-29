using System;
using _Project.Scripts.CurrencyController;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class RewardService : IDisposable
    {
        private readonly ICurrencyController _currencyController;
        private readonly EnemyLibrary _enemyLibrary;

        public RewardService(EnemyLibrary enemyLibrary, ICurrencyController currencyController)
        {
            _enemyLibrary = enemyLibrary;
            _currencyController = currencyController;

            LevelCompletedSignal.OnLevelCompleted += OnLevelCompleted;
        }

        public void Dispose()
        {
            LevelCompletedSignal.OnLevelCompleted -= OnLevelCompleted;
        }

        private void OnLevelCompleted(int level)
        {
            var softReward = CalculateSoftReward(level);
            ApplyReward(softReward, CurrencyType.SoftCurrency);
        }

        private float CalculateSoftReward(int level)
        {
            return MathfRound.Round(_enemyLibrary.EnemyReward * Mathf.Pow(1.12f, level - 1));
        }

        private void ApplyReward(float reward, CurrencyType currencyType)
        {
            _currencyController.AddCurrency(reward, currencyType);
        }
    }
}
