using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.UI
{
    public class DailyQuestsView : MonoBehaviour
    {
        private const string ProgressFormat = "{0}/{1}";
        private const string ClaimLabel = "Забрать";
        private const string ClaimedMark = "✓";
        private const string TimerFormat = "{0}:{1:00} до обновления";
        private const string DiamondFormat = "💎 {0}";
        private const string BonusDiamondFormat = " + 💎 {0}";
        private const int MinutesPerHour = 60;
        private const double SecondsPerMinute = 60.0;
        private const float TimerUpdateInterval = 1f;
        private const float TimerRowHeight = 48f;

        private static readonly Color RowBackgroundColor = new Color32(0, 0, 0, 40);
        private static readonly Color ButtonColor = new Color32(70, 70, 70, 255);
        private static readonly Color ClaimedColor = new Color32(0x33, 0xCC, 0x33, 0xFF); // #3C3

        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _panelRoot; // hidden by default
        [SerializeField] private RectTransform _rowsContainer; // VerticalLayoutGroup is added in the scene

        [Header("Row layout")] [SerializeField] private float _rowHeight = 64f;

        [SerializeField] private int _fontSize = 28;
        [SerializeField] private float _progressWidth = 110f;
        [SerializeField] private float _rewardWidth = 130f;
        [SerializeField] private float _buttonWidth = 110f;

        private readonly List<QuestRow> _rows = new List<QuestRow>();
        private TextMeshProUGUI _timerText; // header, created programmatically above the rows
        private float _nextTimerUpdate;

        private DailyQuestService _quests;

        [Inject]
        private void Construct(DailyQuestService quests)
        {
            _quests = quests;
        }

        private void Awake()
        {
            _panelRoot.SetActive(false);
            _toggleButton.onClick.AddListener(TogglePanel);

            _timerText = CreateText(_rowsContainer, string.Empty, _fontSize, TextAlignmentOptions.Center);
            _timerText.GetComponent<LayoutElement>().preferredHeight = TimerRowHeight;

            RebuildRows();
        }

        private void OnEnable()
        {
            QuestsChangedSignal.OnQuestsChanged += OnQuestsChanged;
        }

        private void OnDisable()
        {
            QuestsChangedSignal.OnQuestsChanged -= OnQuestsChanged;
        }

        private void Update()
        {
            // The timer ticks only while the panel is open, once per second
            if (!_panelRoot.activeSelf || Time.unscaledTime < _nextTimerUpdate)
            {
                return;
            }

            _nextTimerUpdate = Time.unscaledTime + TimerUpdateInterval;
            RefreshTimer();
        }

        private void TogglePanel()
        {
            var isOpen = !_panelRoot.activeSelf;
            _panelRoot.SetActive(isOpen);

            if (isOpen)
            {
                // Data may be stale after the panel was closed for a while
                SyncRows();
                RefreshTimer();
            }

            _nextTimerUpdate = Time.unscaledTime + TimerUpdateInterval;
        }

        private void OnQuestsChanged()
        {
            if (_panelRoot.activeSelf)
            {
                SyncRows();
            }
        }

        // Rebuild is allowed only when the set of quest ids changed, otherwise refresh texts
        private void SyncRows()
        {
            if (_quests == null || _quests.Quests == null)
            {
                return;
            }

            if (HasIdSetChanged())
            {
                RebuildRows();
                return;
            }

            RefreshRows();
        }

        private bool HasIdSetChanged()
        {
            var quests = _quests.Quests;

            if (_rows.Count != quests.Count)
            {
                return true;
            }

            for (var index = 0; index < quests.Count; index++)
            {
                if (_rows[index].Id != quests[index].Id)
                {
                    return true;
                }
            }

            return false;
        }

        private void RebuildRows()
        {
            foreach (var row in _rows)
            {
                Destroy(row.Root);
            }

            _rows.Clear();

            if (_quests == null || _quests.Quests == null)
            {
                return;
            }

            foreach (var quest in _quests.Quests)
            {
                _rows.Add(CreateRow(quest.Id));
            }

            RefreshRows();
        }

        private void RefreshRows()
        {
            if (_quests == null || _quests.Quests == null)
            {
                return;
            }

            for (var index = 0; index < _rows.Count && index < _quests.Quests.Count; index++)
            {
                RefreshRow(_rows[index], _quests.Quests[index]);
            }
        }

        private void RefreshRow(QuestRow row, DailyQuest quest)
        {
            row.DescriptionText.text = quest.Description;
            row.ProgressText.text = string.Format(ProgressFormat,
                Mathf.FloorToInt(quest.Progress), Mathf.FloorToInt(quest.Goal));
            row.RewardText.text = FormatReward(quest);

            row.ClaimButton.interactable = quest.Completed && !quest.Claimed;
            row.ButtonLabel.text = quest.Claimed ? ClaimedMark : ClaimLabel;
            row.ButtonLabel.color = quest.Claimed ? ClaimedColor : Color.white;
        }

        private static string FormatReward(DailyQuest quest)
        {
            var reward = quest.RewardType == CurrencyType.HardCurrency
                ? string.Format(DiamondFormat, Mathf.FloorToInt(quest.RewardAmount))
                : Mathf.FloorToInt(quest.RewardAmount).ToString();

            return quest.BonusDiamonds > 0
                ? reward + string.Format(BonusDiamondFormat, quest.BonusDiamonds)
                : reward;
        }

        private void OnClaimClicked(QuestRow row)
        {
            if (_quests == null || _quests.Quests == null)
            {
                return;
            }

            foreach (var quest in _quests.Quests)
            {
                if (quest.Id == row.Id)
                {
                    // Emits QuestsChangedSignal which refreshes the rows
                    _quests.TryClaim(quest);
                    return;
                }
            }
        }

        private void RefreshTimer()
        {
            var remainingSeconds = _quests != null ? _quests.SecondsRemaining : 0.0;
            var totalMinutes = (int)(remainingSeconds / SecondsPerMinute);

            _timerText.text = string.Format(TimerFormat, totalMinutes / MinutesPerHour, totalMinutes % MinutesPerHour);
        }

        private QuestRow CreateRow(string questId)
        {
            var rowObject = new GameObject("QuestRow", typeof(RectTransform), typeof(Image),
                typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            var rect = (RectTransform)rowObject.transform;
            rect.SetParent(_rowsContainer, false);

            var background = rowObject.GetComponent<Image>();
            background.color = RowBackgroundColor;
            background.raycastTarget = false;

            var layout = rowObject.GetComponent<HorizontalLayoutGroup>();
            layout.padding = new RectOffset(12, 12, 8, 8);
            layout.spacing = 8f;
            layout.childAlignment = TextAnchor.MiddleLeft;
            layout.childForceExpandWidth = false;
            layout.childForceExpandHeight = true;
            layout.childControlWidth = true;
            layout.childControlHeight = true;

            var rowElement = rowObject.GetComponent<LayoutElement>();
            rowElement.preferredHeight = _rowHeight;

            // Fallback for a container without child height control
            rect.sizeDelta = new Vector2(rect.sizeDelta.x, _rowHeight);

            var row = new QuestRow { Id = questId, Root = rowObject };

            row.DescriptionText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Left);
            row.DescriptionText.GetComponent<LayoutElement>().flexibleWidth = 1f;

            row.ProgressText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Right);
            row.ProgressText.GetComponent<LayoutElement>().preferredWidth = _progressWidth;

            row.RewardText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Right);
            row.RewardText.GetComponent<LayoutElement>().preferredWidth = _rewardWidth;

            var (button, label) = CreateClaimButton(rect);
            row.ClaimButton = button;
            row.ButtonLabel = label;
            button.onClick.AddListener(() => OnClaimClicked(row));

            return row;
        }

        private (Button button, TextMeshProUGUI label) CreateClaimButton(RectTransform parent)
        {
            var buttonObject = new GameObject("ClaimButton", typeof(RectTransform), typeof(Image), typeof(Button),
                typeof(LayoutElement));
            var rect = (RectTransform)buttonObject.transform;
            rect.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = ButtonColor;
            image.raycastTarget = true;

            var element = buttonObject.GetComponent<LayoutElement>();
            element.preferredWidth = _buttonWidth;
            element.preferredHeight = _rowHeight - 16f;

            var label = CreateText(rect, ClaimLabel, _fontSize, TextAlignmentOptions.Center);
            label.color = Color.white;
            label.rectTransform.anchorMin = Vector2.zero;
            label.rectTransform.anchorMax = Vector2.one;
            label.rectTransform.offsetMin = Vector2.zero;
            label.rectTransform.offsetMax = Vector2.zero;

            return (buttonObject.GetComponent<Button>(), label);
        }

        private static TextMeshProUGUI CreateText(RectTransform parent, string label, int fontSize,
            TextAlignmentOptions alignment)
        {
            var textObject = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI),
                typeof(LayoutElement));
            var rect = (RectTransform)textObject.transform;
            rect.SetParent(parent, false);

            var text = textObject.GetComponent<TextMeshProUGUI>();
            text.text = label;
            text.fontSize = fontSize;
            text.alignment = alignment;
            text.raycastTarget = false;

            return text;
        }

        private class QuestRow
        {
            public string Id;
            public GameObject Root;
            public TextMeshProUGUI DescriptionText;
            public TextMeshProUGUI ProgressText;
            public TextMeshProUGUI RewardText;
            public Button ClaimButton;
            public TextMeshProUGUI ButtonLabel;
        }
    }
}
