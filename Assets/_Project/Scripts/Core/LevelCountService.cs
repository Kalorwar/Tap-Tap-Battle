using System;
using R3;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public static class LevelCompletedSignal
    {
        private static readonly Subject<int> OnLevelCompletedSubject = new();
        public static Observable<int> OnLevelCompleted => OnLevelCompletedSubject;

        public static void Emit(int level)
        {
            OnLevelCompletedSubject.OnNext(level);
        }
    }

    public class LevelCountService : IDisposable
    {
        private const string LevelNumberSaveKey = "LevelNumberSaveKey";
        private readonly IDisposable _subscription;

        public LevelCountService()
        {
            Load();
            _subscription = EnemyDiedEvent.OnEnemyDead.Subscribe(_ => IncreaseLevel());
        }

        public int LevelCount { get; private set; }

        public void Dispose()
        {
            _subscription?.Dispose();
        }

        public void IncreaseLevel()
        {
            LevelCount++;
            LevelCompletedSignal.Emit(LevelCount);
            Debug.Log($"Level {LevelCount} has been completed");
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