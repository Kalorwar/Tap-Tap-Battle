using System.Collections.Generic;
using _Project.Scripts.Achievements;
using _Project.Scripts.Events;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.UI
{
    public class AchievementsView : MonoBehaviour
    {
        private const string ProgressFormat = "{0}/{1}";
        private const string RewardFormat = "💎 {0}";
        private const string UnlockedMark = "✓";
        private const string ToastFormat = "Достижение: {0} +💎{1}";
        private const float ToastShowSeconds = 2f;
        private const float ToastFadeInDuration = 0.25f;
        private const float ToastFadeOutDuration = 0.5f;
        private const float ToastTopOffset = 120f;
        private const float ToastHeight = 90f;
        private const float ToastHorizontalInset = 128f;

        private static readonly Color RowBackgroundColor = new Color32(0, 0, 0, 40);
        private static readonly Color UnlockedColor = new Color32(0x33, 0xCC, 0x33, 0xFF); // #3C3

        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _panelRoot; // hidden by default
        [SerializeField] private RectTransform _rowsContainer; // VerticalLayoutGroup is added in the scene

        [Header("Row layout")] [SerializeField] private float _rowHeight = 64f;

        [SerializeField] private int _fontSize = 28;
        [SerializeField] private float _progressWidth = 110f;
        [SerializeField] private float _rewardWidth = 90f;
        [SerializeField] private float _statusWidth = 50f;

        private readonly List<AchievementRow> _rows = new List<AchievementRow>();

        private AchievementService _achievements;

        [Inject]
        private void Construct(AchievementService achievements)
        {
            _achievements = achievements;
        }

        private void Awake()
        {
            _panelRoot.SetActive(false);
            _toggleButton.onClick.AddListener(TogglePanel);

            RebuildRows();
        }

        private void OnEnable()
        {
            AchievementsChangedSignal.OnAchievementsChanged += OnAchievementsChanged;
            AchievementUnlockedSignal.OnAchievementUnlocked += OnAchievementUnlocked;
        }

        private void OnDisable()
        {
            AchievementsChangedSignal.OnAchievementsChanged -= OnAchievementsChanged;
            AchievementUnlockedSignal.OnAchievementUnlocked -= OnAchievementUnlocked;
        }

        private void TogglePanel()
        {
            var isOpen = !_panelRoot.activeSelf;
            _panelRoot.SetActive(isOpen);

            if (isOpen)
            {
                // Data may be stale after the panel was closed for a while
                SyncRows();
            }
        }

        private void OnAchievementsChanged()
        {
            if (_panelRoot.activeSelf)
            {
                SyncRows();
            }
        }

        private void OnAchievementUnlocked(AchievementInfo info)
        {
            ShowToast(info);
        }

        // Rebuild is allowed only when the number of achievements changed, otherwise refresh texts
        private void SyncRows()
        {
            if (_achievements == null || _achievements.Entries == null)
            {
                return;
            }

            if (_rows.Count != _achievements.Entries.Count)
            {
                RebuildRows();
                return;
            }

            RefreshRows();
        }

        private void RebuildRows()
        {
            foreach (var row in _rows)
            {
                Destroy(row.Root);
            }

            _rows.Clear();

            if (_achievements == null || _achievements.Entries == null)
            {
                return;
            }

            foreach (var entry in _achievements.Entries)
            {
                _rows.Add(CreateRow());
            }

            RefreshRows();
        }

        private void RefreshRows()
        {
            if (_achievements == null || _achievements.Entries == null)
            {
                return;
            }

            for (var index = 0; index < _rows.Count && index < _achievements.Entries.Count; index++)
            {
                RefreshRow(_rows[index], _achievements.Entries[index]);
            }
        }

        private void RefreshRow(AchievementRow row, AchievementEntry entry)
        {
            row.NameText.text = entry.DisplayName;
            row.ProgressText.text = string.Format(ProgressFormat,
                Mathf.FloorToInt(entry.Progress), Mathf.FloorToInt(entry.Goal));
            row.RewardText.text = string.Format(RewardFormat, entry.DiamondReward);
            row.StatusText.text = entry.IsUnlocked ? UnlockedMark : string.Empty;
            row.StatusText.color = UnlockedColor;
        }

        private AchievementRow CreateRow()
        {
            var rowObject = new GameObject("AchievementRow", typeof(RectTransform), typeof(Image),
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

            var row = new AchievementRow { Root = rowObject };

            row.NameText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Left);
            row.NameText.GetComponent<LayoutElement>().flexibleWidth = 1f;

            row.ProgressText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Right);
            row.ProgressText.GetComponent<LayoutElement>().preferredWidth = _progressWidth;

            row.RewardText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Right);
            row.RewardText.GetComponent<LayoutElement>().preferredWidth = _rewardWidth;

            row.StatusText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Center);
            row.StatusText.GetComponent<LayoutElement>().preferredWidth = _statusWidth;

            return row;
        }

        private void ShowToast(AchievementInfo info)
        {
            var toastObject = new GameObject("AchievementToast", typeof(RectTransform), typeof(CanvasGroup),
                typeof(TextMeshProUGUI));
            var rect = (RectTransform)toastObject.transform;

            // The toast lives directly on the canvas, top-stretch, regardless of the panel state
            var canvas = GetComponentInParent<Canvas>();
            rect.SetParent(canvas != null ? canvas.transform : transform, false);
            rect.anchorMin = new Vector2(0f, 1f);
            rect.anchorMax = new Vector2(1f, 1f);
            rect.pivot = new Vector2(0.5f, 1f);
            rect.sizeDelta = new Vector2(-ToastHorizontalInset, ToastHeight);
            rect.anchoredPosition = new Vector2(0f, -ToastTopOffset);

            var text = toastObject.GetComponent<TextMeshProUGUI>();
            text.text = string.Format(ToastFormat, info.DisplayName, info.DiamondReward);
            text.fontSize = 40;
            text.alignment = TextAlignmentOptions.Center;
            text.raycastTarget = false;

            var canvasGroup = toastObject.GetComponent<CanvasGroup>();
            canvasGroup.alpha = 0f;

            // Show, hold for a while, fade out and destroy
            var sequence = DOTween.Sequence();
            sequence.Append(canvasGroup.DOFade(1f, ToastFadeInDuration));
            sequence.AppendInterval(ToastShowSeconds);
            sequence.Append(canvasGroup.DOFade(0f, ToastFadeOutDuration));
            sequence.OnComplete(() => Destroy(toastObject));
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

        private class AchievementRow
        {
            public GameObject Root;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI ProgressText;
            public TextMeshProUGUI RewardText;
            public TextMeshProUGUI StatusText;
        }
    }
}
