using System;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public static class LevelCompletedSignal
    {
        public static event Action<int> OnLevelCompleted;

        public static void Emit(int level)
        {
            OnLevelCompleted?.Invoke(level);
        }
    }

    public class LevelCountService : IDisposable
    {
        private const string LevelNumberSaveKey = "LevelNumberSaveKey";

        public LevelCountService()
        {
            Load();
            EnemyDiedEvent.OnEnemyDead += OnEnemyDead;
        }

        public int LevelCount { get; private set; }

        public void Dispose()
        {
            EnemyDiedEvent.OnEnemyDead -= OnEnemyDead;
        }

        public void IncreaseLevel()
        {
            Debug.Log($"Level {LevelCount} has been completed");
            LevelCount++;
            LevelCompletedSignal.Emit(LevelCount);
            Save();
        }

        public void SetLevel(int level)
        {
            if (level < 1)
            {
                level = 1;
            }

            LevelCount = level;
            Save();
        }

        private void OnEnemyDead()
        {
            IncreaseLevel();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(LevelNumberSaveKey, LevelCount);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            LevelCount = PlayerPrefs.GetInt(LevelNumberSaveKey, 1);
            Debug.Log($"Level {LevelCount} has been loaded");
        }
    }
}
