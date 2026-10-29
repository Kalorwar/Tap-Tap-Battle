using System;
using _Project.Scripts.CurrencyController;
using R3;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class RewardService : IDisposable
    {
        private readonly ICurrencyController _currencyController;
        private readonly EnemyLibrary _enemyLibrary;
        private readonly CompositeDisposable _subscriptions = new();

        public RewardService(EnemyLibrary enemyLibrary, ICurrencyController currencyController)
        {
            _enemyLibrary = enemyLibrary;
            _currencyController = currencyController;

            LevelCompletedSignal.OnLevelCompleted
                .Subscribe(level =>
                {
                    var softReward = CalculateSoftReward(level);
                    ApplyReward(softReward, CurrencyType.SoftCurrency);
                    if (level % 11 == 0)
                    {
                        var hardReward = CalculateHardReward(level);
                        ApplyReward(hardReward, CurrencyType.HardCurrency);
                    }
                })
                .AddTo(_subscriptions);
        }

        public void Dispose()
        {
            _subscriptions?.Dispose();
        }

        private float CalculateSoftReward(int level)
        {
            return MathfRound.Round(_enemyLibrary.EnemyReward * Mathf.Pow(1.12f, level - 1));
        }

        private float CalculateHardReward(int level)
        {
            return MathfRound.Round(_enemyLibrary.BossReward * Mathf.Pow(1f, level - 1));
        }

        private void ApplyReward(float reward, CurrencyType currencyType)
        {
            _currencyController.AddCurrency(reward, currencyType);
        }
    }
}