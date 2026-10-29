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
        private const int KillsPerLevel = 5;
        private const int BossLevelInterval = 10;

        private const string LevelSaveKey = "Progression.Level";
        private const string KillsOnLevelSaveKey = "Progression.KillsOnLevel";
        private const string BossesDefeatedSaveKey = "Progression.BossesDefeated";

        public LevelCountService()
        {
            Load();
            EnemyKilledSignal.OnEnemyKilled += OnEnemyKilled;
        }

        public int LevelCount { get; private set; }

        public int KillsOnLevel { get; private set; }

        public int BossesDefeated { get; private set; }

        // Every 10th level is a single boss fight instead of 5 enemies
        public bool IsBossLevel => LevelCount % BossLevelInterval == 0;

        public void Dispose()
        {
            EnemyKilledSignal.OnEnemyKilled -= OnEnemyKilled;
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

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            if (info.IsBoss)
            {
                CompleteBossLevel();
                return;
            }

            KillsOnLevel++;
            if (KillsOnLevel < KillsPerLevel)
            {
                Save();
                return;
            }

            CompleteLevel();
        }

        private void CompleteLevel()
        {
            Debug.Log($"Level {LevelCount} has been completed");
            LevelCount++;
            KillsOnLevel = 0;
            Save();
            LevelCompletedSignal.Emit(LevelCount);
        }

        private void CompleteBossLevel()
        {
            BossesDefeated++;
            Save();
            BossDefeatedSignal.Emit(BossesDefeated);
            CompleteLevel();
        }

        private void Save()
        {
            PlayerPrefs.SetInt(LevelSaveKey, LevelCount);
            PlayerPrefs.SetInt(KillsOnLevelSaveKey, KillsOnLevel);
            PlayerPrefs.SetInt(BossesDefeatedSaveKey, BossesDefeated);
            PlayerPrefs.Save();
        }

        private void Load()
        {
            LevelCount = Mathf.Max(1, PlayerPrefs.GetInt(LevelSaveKey, 1));
            KillsOnLevel = Mathf.Clamp(PlayerPrefs.GetInt(KillsOnLevelSaveKey, 0), 0, KillsPerLevel - 1);
            BossesDefeated = Mathf.Max(0, PlayerPrefs.GetInt(BossesDefeatedSaveKey, 0));
            Debug.Log($"Level {LevelCount} with {KillsOnLevel} kills has been loaded");
        }
    }
}
