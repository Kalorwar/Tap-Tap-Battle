using System;
using System.Collections.Generic;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class DailyQuestService : IDisposable
    {
        private const string ActiveIdsKey = "Quests.ActiveIds";
        private const string WindowStartKey = "Quests.WindowStartUtc";
        private const string ProgressKeyFormat = "Quests.Progress.{0}";
        private const string ClaimedKeyFormat = "Quests.Claimed.{0}";

        private const int ActiveQuestCount = 3;
        private const float WindowDurationHours = 24f;

        private const string KillQuestId = "kill20";
        private const string BossQuestId = "boss2";
        private const string TapQuestId = "tap1000";
        private const string UpgradeQuestId = "upgrade5";

        // Quest pool from the GDD; bonusDiamonds > 0 adds an extra hard currency reward
        private static readonly QuestDefinition[] Pool =
        {
            new QuestDefinition(KillQuestId, "Убей 20 врагов", 20f, CurrencyType.SoftCurrency, 200f, 0),
            new QuestDefinition(BossQuestId, "Победи 2 боссов", 2f, CurrencyType.SoftCurrency, 500f, 3),
            new QuestDefinition(TapQuestId, "Сделай 1000 тапов", 1000f, CurrencyType.SoftCurrency, 300f, 0),
            new QuestDefinition(UpgradeQuestId, "Прокачай помощника 5 раз", 5f, CurrencyType.SoftCurrency, 400f, 0),
        };

        private readonly ICurrencyController _currencyController;
        private readonly List<DailyQuest> _quests = new List<DailyQuest>();

        private DateTime _windowStartUtc;

        public DailyQuestService(ICurrencyController currencyController)
        {
            _currencyController = currencyController;

            if (!TryRestore())
            {
                GenerateWindow();
            }

            EnemyKilledSignal.OnEnemyKilled += OnEnemyKilled;
            BossDefeatedSignal.OnBossDefeated += OnBossDefeated;
            TapEvent.OnTapped += OnTapped;
            AssistantChangedSignal.OnAssistantChanged += OnAssistantChanged;
        }

        public void Dispose()
        {
            EnemyKilledSignal.OnEnemyKilled -= OnEnemyKilled;
            BossDefeatedSignal.OnBossDefeated -= OnBossDefeated;
            TapEvent.OnTapped -= OnTapped;
            AssistantChangedSignal.OnAssistantChanged -= OnAssistantChanged;
        }

        public IReadOnlyList<DailyQuest> Quests => _quests;

        public bool WindowActive => SecondsRemaining > 0d;

        public double SecondsRemaining
        {
            get
            {
                var elapsed = (DateTime.UtcNow - _windowStartUtc).TotalSeconds;
                return Math.Max(0d, WindowDurationHours * 3600d - elapsed);
            }
        }

        public bool TryClaim(DailyQuest quest)
        {
            if (quest == null || !quest.Completed || quest.Claimed)
            {
                return false;
            }

            _currencyController.AddCurrency(quest.RewardAmount, quest.RewardType);
            if (quest.BonusDiamonds > 0)
            {
                _currencyController.AddCurrency(quest.BonusDiamonds, CurrencyType.HardCurrency);
            }

            quest.Claimed = true;
            WriteClaimed(quest);
            Persist();

            QuestsChangedSignal.Emit();
            return true;
        }

        private void OnEnemyKilled(EnemyKilledInfo info)
        {
            AddProgress(KillQuestId, 1f);
        }

        // Only real boss defeats count here; boss kills also come via EnemyKilledSignal but are ignored
        private void OnBossDefeated(int bossIndex)
        {
            AddProgress(BossQuestId, 1f);
        }

        private void OnTapped()
        {
            AddProgress(TapQuestId, 1f);
        }

        private void OnAssistantChanged()
        {
            AddProgress(UpgradeQuestId, 1f);
        }

        private void AddProgress(string id, float amount)
        {
            var quest = FindQuest(id);
            if (quest == null || quest.Completed || quest.Claimed)
            {
                return;
            }

            quest.Progress = Mathf.Min(quest.Progress + amount, quest.Goal);
            WriteProgress(quest);

            if (quest.Progress >= quest.Goal)
            {
                quest.Completed = true;
                QuestsChangedSignal.Emit();
            }

            Persist();
        }

        private DailyQuest FindQuest(string id)
        {
            foreach (var quest in _quests)
            {
                if (quest.Id == id)
                {
                    return quest;
                }
            }

            return null;
        }

        // Restores the active quests while the 24h window is still running
        private bool TryRestore()
        {
            _windowStartUtc = DateTime.FromBinary(PlayerPrefsLong.Get(WindowStartKey, 0L));
            if ((DateTime.UtcNow - _windowStartUtc).TotalHours >= WindowDurationHours)
            {
                return false;
            }

            var ids = PlayerPrefs.GetString(ActiveIdsKey, string.Empty);
            if (string.IsNullOrEmpty(ids))
            {
                return false;
            }

            _quests.Clear();
            foreach (var id in ids.Split(';'))
            {
                var definition = FindDefinition(id);
                if (definition == null)
                {
                    _quests.Clear();
                    return false;
                }

                var progress = PlayerPrefs.GetFloat(GetProgressKey(id), 0f);
                var claimed = PlayerPrefs.GetInt(GetClaimedKey(id), 0) == 1;
                _quests.Add(CreateQuest(definition.Value, progress, claimed));
            }

            return _quests.Count == ActiveQuestCount;
        }

        // Picks random distinct quests from the pool and starts a fresh 24h window
        private void GenerateWindow()
        {
            ClearQuestKeys();

            var pool = new List<QuestDefinition>(Pool);
            var ids = new List<string>(ActiveQuestCount);
            _quests.Clear();

            for (var i = 0; i < ActiveQuestCount; i++)
            {
                var index = UnityEngine.Random.Range(0, pool.Count);
                var definition = pool[index];
                pool.RemoveAt(index);

                var quest = CreateQuest(definition, 0f, false);
                _quests.Add(quest);
                ids.Add(quest.Id);

                WriteProgress(quest);
                WriteClaimed(quest);
            }

            _windowStartUtc = DateTime.UtcNow;
            PlayerPrefs.SetString(ActiveIdsKey, string.Join(";", ids));
            PlayerPrefsLong.Set(WindowStartKey, _windowStartUtc.ToBinary());
            Persist();

            QuestsChangedSignal.Emit();
        }

        private void ClearQuestKeys()
        {
            foreach (var quest in _quests)
            {
                PlayerPrefs.DeleteKey(GetProgressKey(quest.Id));
                PlayerPrefs.DeleteKey(GetClaimedKey(quest.Id));
            }
        }

        private static DailyQuest CreateQuest(QuestDefinition definition, float progress, bool claimed)
        {
            return new DailyQuest(definition.Id, definition.Description, definition.Goal, definition.RewardType,
                definition.RewardAmount, definition.BonusDiamonds, progress, claimed);
        }

        private static QuestDefinition? FindDefinition(string id)
        {
            foreach (var definition in Pool)
            {
                if (definition.Id == id)
                {
                    return definition;
                }
            }

            return null;
        }

        private static string GetProgressKey(string id)
        {
            return string.Format(ProgressKeyFormat, id);
        }

        private static string GetClaimedKey(string id)
        {
            return string.Format(ClaimedKeyFormat, id);
        }

        private static void WriteProgress(DailyQuest quest)
        {
            PlayerPrefs.SetFloat(GetProgressKey(quest.Id), quest.Progress);
        }

        private static void WriteClaimed(DailyQuest quest)
        {
            PlayerPrefs.SetInt(GetClaimedKey(quest.Id), quest.Claimed ? 1 : 0);
        }

        private static void Persist()
        {
            PlayerPrefs.Save();
        }

        private readonly struct QuestDefinition
        {
            public readonly string Id;
            public readonly string Description;
            public readonly float Goal;
            public readonly CurrencyType RewardType;
            public readonly float RewardAmount;
            public readonly int BonusDiamonds;

            public QuestDefinition(string id, string description, float goal, CurrencyType rewardType,
                float rewardAmount, int bonusDiamonds)
            {
                Id = id;
                Description = description;
                Goal = goal;
                RewardType = rewardType;
                RewardAmount = rewardAmount;
                BonusDiamonds = bonusDiamonds;
            }
        }
    }

    public sealed class DailyQuest
    {
        public DailyQuest(string id, string description, float goal, CurrencyType rewardType,
            float rewardAmount, int bonusDiamonds, float progress, bool claimed)
        {
            Id = id;
            Description = description;
            Goal = goal;
            RewardType = rewardType;
            RewardAmount = rewardAmount;
            BonusDiamonds = bonusDiamonds;
            Progress = progress;
            Claimed = claimed;
            Completed = Progress >= Goal;
        }

        public string Id { get; }
        public string Description { get; }
        public float Goal { get; }
        public CurrencyType RewardType { get; }
        public float RewardAmount { get; }
        public int BonusDiamonds { get; }

        public float Progress { get; internal set; }
        public bool Completed { get; internal set; }
        public bool Claimed { get; internal set; }
    }
}
