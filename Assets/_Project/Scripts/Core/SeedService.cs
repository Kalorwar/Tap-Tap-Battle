using System;
using R3;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class SeedService : IDisposable
    {
        private const string SeedSaveKey = "EnemySpawnSeed";
        private readonly CompositeDisposable _subscription = new();

        public SeedService()
        {
            LevelCompletedSignal.OnLevelCompleted.Subscribe(_ => ChangeSeed())
                .AddTo(_subscription);
            Load();
        }

        public int CurrentSeed { get; private set; }

        public void Dispose()
        {
        }

        private void ChangeSeed()
        {
            CurrentSeed += 1;
            Save();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(SeedSaveKey, CurrentSeed);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            CurrentSeed = PlayerPrefs.GetInt(SeedSaveKey, 1);
        }
    }
}