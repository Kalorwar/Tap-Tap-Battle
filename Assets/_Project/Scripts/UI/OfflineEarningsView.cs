using _Project.Scripts.Core;
using _Project.Scripts.Events;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Zenject;

namespace _Project.Scripts.UI
{
    public class OfflineEarningsView : MonoBehaviour
    {
        private const string AmountFormat = "{0} монет";
        private const string AwayHoursFormat = "Вас не было {0} ч {1} мин";
        private const string AwayMinutesFormat = "Вас не было {0} мин";
        private const int MinutesPerHour = 60;
        private const float SecondsPerMinute = 60f;

        [SerializeField] private GameObject _panelRoot; // hidden by default
        [SerializeField] private TextMeshProUGUI _amountText;
        [SerializeField] private TextMeshProUGUI _awayText;
        [SerializeField] private Button _claimButton;

        private OfflineProgressService _offline;

        [Inject]
        private void Construct(OfflineProgressService offline)
        {
            _offline = offline;
        }

        private void Awake()
        {
            _panelRoot.SetActive(false);
            _claimButton.onClick.AddListener(OnClaimClicked);
        }

        private void OnEnable()
        {
            OfflineProgressSignal.OnOfflineProgress += OnOfflineProgress;
            Refresh();
        }

        private void OnDisable()
        {
            OfflineProgressSignal.OnOfflineProgress -= OnOfflineProgress;
        }

        private void Start()
        {
            // Fallback in case the injection ran after OnEnable
            Refresh();
        }

        private void OnOfflineProgress()
        {
            // Claim happened elsewhere: hide the popup
            Hide();
        }

        private void OnClaimClicked()
        {
            if (_offline == null || _offline.PendingCoins <= 0f)
            {
                Hide();
                return;
            }

            // Emits OfflineProgressSignal which hides the popup as well
            _offline.Claim();
            Hide();
        }

        private void Refresh()
        {
            if (_offline == null || _offline.PendingCoins <= 0f)
            {
                return;
            }

            _amountText.text = string.Format(AmountFormat, Mathf.FloorToInt(_offline.PendingCoins));
            _awayText.text = FormatAwayTime(_offline.AwaySeconds);
            _panelRoot.SetActive(true);
        }

        private void Hide()
        {
            _panelRoot.SetActive(false);
        }

        private static string FormatAwayTime(double awaySeconds)
        {
            var totalMinutes = Mathf.FloorToInt((float)(awaySeconds / SecondsPerMinute));
            var hours = totalMinutes / MinutesPerHour;
            var minutes = totalMinutes % MinutesPerHour;

            return hours < 1
                ? string.Format(AwayMinutesFormat, minutes)
                : string.Format(AwayHoursFormat, hours, minutes);
        }
    }
}
