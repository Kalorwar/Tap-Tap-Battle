using System.Collections.Generic;
using _Project.Scripts.Assistants;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using _Project.Scripts.Upgrades;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.UI
{
    public class UpgradeHubView : MonoBehaviour
    {
        private const string DamageRowName = "Урон";
        private const string BuyLabel = "Купить";
        private const string HireLabel = "Нанять";
        private const string ImproveLabel = "Улучшить";
        private const string LevelFormat = "ур.{0}";
        private const string TapEffectFormat = "{0}/тап";
        private const string DpsEffectFormat = "{0} DPS";

        private static readonly Color AffordableColor = new Color32(0x33, 0xCC, 0x33, 0xFF); // #3C3
        private static readonly Color TooExpensiveColor = new Color32(0xCC, 0x33, 0x33, 0xFF); // #C33
        private static readonly Color RowBackgroundColor = new Color32(0, 0, 0, 40);
        private static readonly Color ButtonColor = new Color32(70, 70, 70, 255);

        [SerializeField] private GameObject _panelRoot; // hidden by default
        [SerializeField] private Button _toggleButton;
        [SerializeField] private RectTransform _rowsContainer; // VerticalLayoutGroup is added in the scene

        [Header("Row layout")] [SerializeField] private float _rowHeight = 64f;

        [SerializeField] private int _fontSize = 28;
        [SerializeField] private float _levelWidth = 70f;
        [SerializeField] private float _costWidth = 90f;
        [SerializeField] private float _buttonWidth = 110f;

        private readonly List<UpgradeRow> _rows = new List<UpgradeRow>();

        private TapDamageUpgradeService _tapUpgrade;
        private AssistantService _assistantService;
        private IReadOnlyList<AssistantConfig> _assistantConfigs;
        private ICurrencyController _currency;

        [Inject]
        private void Construct(TapDamageUpgradeService tapUpgrade, AssistantService assistantService,
            IReadOnlyList<AssistantConfig> assistantConfigs, ICurrencyController currency)
        {
            _tapUpgrade = tapUpgrade;
            _assistantService = assistantService;
            _assistantConfigs = assistantConfigs;
            _currency = currency;
        }

        private void Awake()
        {
            _panelRoot.SetActive(false);
            _toggleButton.onClick.AddListener(TogglePanel);

            // Rows are built once; later updates only refresh texts/buttons of existing rows
            RebuildRows();
        }

        private void OnEnable()
        {
            UpgradeChangedSignal.OnUpgradeChanged += OnUpgradeChanged;
            AssistantChangedSignal.OnAssistantChanged += OnAssistantChanged;
            _currency.OnCurrencyChange += OnUpgradeChanged;

            if (_panelRoot.activeSelf)
            {
                RefreshRows();
            }
        }

        private void OnDisable()
        {
            UpgradeChangedSignal.OnUpgradeChanged -= OnUpgradeChanged;
            AssistantChangedSignal.OnAssistantChanged -= OnAssistantChanged;
            if (_currency != null)
            {
                _currency.OnCurrencyChange -= OnUpgradeChanged;
            }
        }

        private void TogglePanel()
        {
            var isOpen = !_panelRoot.activeSelf;
            _panelRoot.SetActive(isOpen);

            // Data may be stale after the panel was closed for a while
            if (isOpen)
            {
                RefreshRows();
            }
        }

        private void OnUpgradeChanged()
        {
            if (_panelRoot.activeSelf)
            {
                RefreshRows();
            }
        }

        private void OnAssistantChanged()
        {
            // Rebuild is allowed only when the set of unlocked assistants actually changed
            if (HasUnlockedSetChanged())
            {
                RebuildRows();
                return;
            }

            if (_panelRoot.activeSelf)
            {
                RefreshRows();
            }
        }

        private bool HasUnlockedSetChanged()
        {
            var unlocked = GetUnlockedConfigs();
            var index = 0;

            foreach (var row in _rows)
            {
                if (row.Config == null)
                {
                    continue;
                }

                if (index >= unlocked.Count || !ReferenceEquals(row.Config, unlocked[index]))
                {
                    return true;
                }

                index++;
            }

            return index != unlocked.Count;
        }

        private List<AssistantConfig> GetUnlockedConfigs()
        {
            var unlocked = new List<AssistantConfig>();

            foreach (var config in _assistantConfigs)
            {
                if (_assistantService.IsUnlocked(config))
                {
                    unlocked.Add(config);
                }
            }

            return unlocked;
        }

        private void RebuildRows()
        {
            foreach (var row in _rows)
            {
                Destroy(row.Root);
            }

            _rows.Clear();

            // Row 0 is always the tap damage upgrade
            _rows.Add(CreateRow(null));

            foreach (var config in GetUnlockedConfigs())
            {
                _rows.Add(CreateRow(config));
            }

            RefreshRows();
        }

        private void RefreshRows()
        {
            foreach (var row in _rows)
            {
                if (row.Config == null)
                {
                    RefreshTapRow(row);
                }
                else
                {
                    RefreshAssistantRow(row);
                }
            }
        }

        private void RefreshTapRow(UpgradeRow row)
        {
            var affordable = _tapUpgrade.CanBuy;

            row.NameText.text = DamageRowName;
            row.LevelText.text = string.Format(LevelFormat, _tapUpgrade.Level);
            row.EffectText.text = string.Format(TapEffectFormat, Mathf.FloorToInt(_tapUpgrade.CurrentDamage));
            row.CostText.text = Mathf.FloorToInt(_tapUpgrade.NextCost).ToString();
            row.ButtonLabel.text = BuyLabel;

            row.CostText.color = affordable ? AffordableColor : TooExpensiveColor;
            row.BuyButton.interactable = affordable;
        }

        private void RefreshAssistantRow(UpgradeRow row)
        {
            var config = row.Config;
            var level = _assistantService.GetLevel(config.Id);
            var cost = _assistantService.GetCost(config);
            var affordable = _currency.HasEnough(cost, CurrencyType.SoftCurrency);

            // Level 0 shows the base DPS and a "hire" label
            var dps = level < 1 ? config.BaseDps : _assistantService.GetDps(config);

            row.NameText.text = config.DisplayName;
            row.LevelText.text = string.Format(LevelFormat, level);
            row.EffectText.text = string.Format(DpsEffectFormat, Mathf.FloorToInt(dps));
            row.CostText.text = Mathf.FloorToInt(cost).ToString();
            row.ButtonLabel.text = level < 1 ? HireLabel : ImproveLabel;

            row.CostText.color = affordable ? AffordableColor : TooExpensiveColor;
            row.BuyButton.interactable = affordable;
        }

        private void OnBuyClicked(UpgradeRow row)
        {
            if (row.Config == null)
            {
                // Emits UpgradeChangedSignal which refreshes the rows
                _tapUpgrade.TryBuy();
                return;
            }

            // Emits AssistantChangedSignal which refreshes or rebuilds the rows
            _assistantService.TryBuy(row.Config);
        }

        private UpgradeRow CreateRow(AssistantConfig config)
        {
            var rowObject = new GameObject("UpgradeRow", typeof(RectTransform), typeof(Image),
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

            var row = new UpgradeRow { Config = config, Root = rowObject };

            row.NameText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Left);
            row.NameText.GetComponent<LayoutElement>().flexibleWidth = 1f;

            row.LevelText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Right);
            row.LevelText.GetComponent<LayoutElement>().preferredWidth = _levelWidth;

            row.EffectText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Right);
            row.EffectText.GetComponent<LayoutElement>().flexibleWidth = 1f;

            row.CostText = CreateText(rect, string.Empty, _fontSize, TextAlignmentOptions.Right);
            row.CostText.GetComponent<LayoutElement>().preferredWidth = _costWidth;

            var (button, label) = CreateBuyButton(rect);
            row.BuyButton = button;
            row.ButtonLabel = label;
            button.onClick.AddListener(() => OnBuyClicked(row));

            return row;
        }

        private (Button button, TextMeshProUGUI label) CreateBuyButton(RectTransform parent)
        {
            var buttonObject = new GameObject("BuyButton", typeof(RectTransform), typeof(Image), typeof(Button),
                typeof(LayoutElement));
            var rect = (RectTransform)buttonObject.transform;
            rect.SetParent(parent, false);

            var image = buttonObject.GetComponent<Image>();
            image.color = ButtonColor;
            image.raycastTarget = true;

            var element = buttonObject.GetComponent<LayoutElement>();
            element.preferredWidth = _buttonWidth;
            element.preferredHeight = _rowHeight - 16f;

            var label = CreateText(rect, BuyLabel, _fontSize, TextAlignmentOptions.Center);
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

        private class UpgradeRow
        {
            public AssistantConfig Config; // null for the tap damage row
            public GameObject Root;
            public TextMeshProUGUI NameText;
            public TextMeshProUGUI LevelText;
            public TextMeshProUGUI EffectText;
            public TextMeshProUGUI CostText;
            public Button BuyButton;
            public TextMeshProUGUI ButtonLabel;
        }
    }
}
