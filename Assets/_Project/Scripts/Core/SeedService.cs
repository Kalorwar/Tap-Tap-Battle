using System;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class SeedService : IDisposable
    {
        private const string SeedSaveKey = "EnemySpawnSeed";

        public SeedService()
        {
            LevelCompletedSignal.OnLevelCompleted += OnLevelCompleted;
            Load();
        }

        public int CurrentSeed { get; private set; }

        public void Dispose()
        {
            LevelCompletedSignal.OnLevelCompleted -= OnLevelCompleted;
        }

        private void OnLevelCompleted(int level)
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
