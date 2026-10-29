using System.Collections.Generic;
using _Project.Scripts.Core;
using _Project.Scripts.Events;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.UI
{
    public class DailyRewardsView : MonoBehaviour
    {
        private const int DayCount = 7;
        private const string DayFormat = "День {0}";
        private const string ClaimLabel = "Забрать";
        private const string ClaimedMark = "✓";
        private const float DimmedAlpha = 0.4f;
        private const float PulseScale = 1.15f;
        private const float PulseDuration = 0.5f;

        private static readonly Color RowBackgroundColor = new Color32(0, 0, 0, 40);
        private static readonly Color ButtonColor = new Color32(70, 70, 70, 255);
        private static readonly Color ClaimedColor = new Color32(0x33, 0xCC, 0x33, 0xFF); // #3C3

        [SerializeField] private Button _toggleButton;
        [SerializeField] private GameObject _panelRoot; // hidden by default
        [SerializeField] private RectTransform _rowsContainer; // VerticalLayoutGroup is added in the scene

        [Header("Row layout")] [SerializeField] private float _rowHeight = 64f;

        [SerializeField] private int _fontSize = 28;
        [SerializeField] private float _dayWidth = 110f;
        [SerializeField] private float _buttonWidth = 110f;

        private readonly List<DailyRewardRow> _rows = new List<DailyRewardRow>();
        private Tween _pulseTween;

        private DailyRewardService _daily;

        [Inject]
        private void Construct(DailyRewardService daily)
        {
            _daily = daily;
        }

        private void Awake()
        {
            _panelRoot.SetActive(false);
            _toggleButton.onClick.AddListener(TogglePanel);

            // Rows are built once; later updates only refresh statuses
            CreateRows();
            RefreshRows();
            UpdatePulse();
        }

        private void OnEnable()
        {
            DailyChangedSignal.OnDailyChanged += OnDailyChanged;
        }

        private void OnDisable()
        {
            DailyChangedSignal.OnDailyChanged -= OnDailyChanged;
            StopPulse();
        }

        private void Start()
        {
            // Fallback in case the injection ran after Awake
            RefreshRows();
            UpdatePulse();
        }

        private void TogglePanel()
        {
            var isOpen = !_panelRoot.activeSelf;
            _panelRoot.SetActive(isOpen);

            if (isOpen)
            {
                // Data may be stale after the panel was closed for a while
                RefreshRows();
            }

            UpdatePulse();
        }

        private void OnDailyChanged()
        {
            if (_panelRoot.activeSelf)
            {
                RefreshRows();
            }

            UpdatePulse();
        }

        private void CreateRows()
        {
            for (var day = 1; day <= DayCount; day++)
            {
                _rows.Add(CreateRow(day));
            }
        }

        private void RefreshRows()
        {
            if (_daily == null)
            {
                return;
            }

            var claimedDays = GetClaimedDaysCount();

            foreach (var row in _rows)
            {
                var isClaimed = row.Day <= claimedDays;
                var isClaimable = row.Day == _daily.NextDay && _daily.CanClaim;

                row.NameText.text = string.Format(DayFormat, row.Day);
                row.DescriptionText.text = _daily.DescribeDay(row.Day);

                row.ClaimButton.interactable = isClaimable;
                row.ButtonLabel.text = isClaimable ? ClaimLabel : (isClaimed ? ClaimedMark : string.Empty);
                row.ButtonLabel.color = isClaimed ? ClaimedColor : Color.white;

                // Future days are dimmed, claimed and claimable days are fully visible
                row.CanvasGroup.alpha = isClaimed || isClaimable ? 1f : DimmedAlpha;
            }
        }

        // Streak may grow beyond the cycle length; wrap it back onto the 7-day calendar
        private int GetClaimedDaysCount()
        {
            var streak = _daily.CurrentStreak;

            if (streak <= 0)
            {
                return 0;
            }

            var claimed = streak % DayCount;
            return claimed == 0 ? DayCount : claimed;
        }

        private void OnClaimClicked()
        {
            if (_daily == null)
            {
                return;
            }

            // Emits DailyChangedSignal which refreshes the rows and the pulse
            _daily.Claim();
        }

        private void UpdatePulse()
        {
            var shouldPulse = _daily != null && _daily.CanClaim && !_panelRoot.activeSelf;

            if (shouldPulse && _pulseTween == null)
            {
                // Endless yoyo pulse while a reward is waiting and the panel is closed
                _pulseTween = _toggleButton.transform.DOScale(new Vector3(PulseScale, PulseScale, PulseScale), PulseDuration)
                    .SetLoops(-1, LoopType.Yoyo)
                    .SetEase(Ease.InOutSine);
            }
            else if (!shouldPulse && _pulseTween != null)
            {
                StopPulse();
            }
        }

        private void StopPulse()
        {
            if (_pulseTween == null)
            {
                return;
            }

            _pulseTween.Kill();
            _pulseTween = null;
            _toggleButton.transform.localScale = Vector3.one;
        }

        private DailyRewardRow CreateRow(int day)
        {
            var rowObject = new GameObject("DailyRewardRow", typeof(RectTransform), typeof(Image),
                typeof(HorizontalLayoutGroup), typeof(LayoutElement), typeof(CanvasGroup));
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

            var row = new DailyRewardRow { Day = day, Root = rowObject, CanvasGroup = rowObject.GetComponent<CanvasGroup>() };

            row.NameText = CreateText(rect, string.Format(DayFormat, day), _fontSize, TextAlignmentOptions.Left);
            row.NameText.GetComponent<LayoutElement>().preferredWidth = _dayWidth;

            row.DescriptionText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Left);
            row.DescriptionText.GetComponent<LayoutElement>().flexibleWidth = 1f;

            var (button, label) = CreateClaimButton(rect);
            row.ClaimButton = button;
            row.ButtonLabel = label;
            button.onClick.AddListener(OnClaimClicked);

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

        private class DailyRewardRow
        {
            public int Day;
            public GameObject Root;
            public CanvasGroup CanvasGroup;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI DescriptionText;
            public Button ClaimButton;
            public TextMeshProUGUI ButtonLabel;
        }
    }
}
