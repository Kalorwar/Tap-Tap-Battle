using System;
using _Project.Scripts.Assistants;
using _Project.Scripts.CurrencyController;
using _Project.Scripts.Events;
using UnityEngine;

namespace _Project.Scripts.Core
{
    /// <summary>
    /// Calculates soft coins earned while the player was away (capped and halved efficiency).
    /// Reads Meta.LastSeenUtc in the constructor; TouchLastSeen keeps the timestamp fresh.
    /// Claim moves pending coins to the soft balance and raises OfflineProgressSignal.
    /// </summary>
    public class OfflineProgressService
    {
        private const string LastSeenKey = "Meta.LastSeenUtc";

        private const long CapSeconds = 7200;
        private const long MinAwaySeconds = 60;
        private const float Efficiency = 0.5f;

        private readonly AssistantService _assistantService;
        private readonly ICurrencyController _currencyController;

        public OfflineProgressService(AssistantService assistantService, ICurrencyController currencyController)
        {
            _assistantService = assistantService;
            _currencyController = currencyController;

            CalculateOfflineProgress();
        }

        public float PendingCoins { get; private set; }

        // Uncapped away time, for "you were away N hours" UI
        public long AwaySeconds { get; private set; }

        public void Claim()
        {
            if (PendingCoins <= 0f)
            {
                return;
            }

            _currencyController.AddCurrency(PendingCoins, CurrencyType.SoftCurrency);
            PendingCoins = 0f;
            OfflineProgressSignal.Emit();
        }

        // Called by MetaSessionTracker on quit / focus loss / heartbeat
        public void TouchLastSeen()
        {
            PlayerPrefsLong.Set(LastSeenKey, DateTime.UtcNow.ToBinary());
            PlayerPrefs.Save();
        }

        private void CalculateOfflineProgress()
        {
            var nowBinary = DateTime.UtcNow.ToBinary();
            if (!PlayerPrefs.HasKey(LastSeenKey))
            {
                PlayerPrefsLong.Set(LastSeenKey, nowBinary);
                PlayerPrefs.Save();
                return;
            }

            var lastBinary = PlayerPrefsLong.Get(LastSeenKey, nowBinary);
            var ticks = Math.Max(0L, nowBinary - lastBinary);
            AwaySeconds = ticks / TimeSpan.TicksPerSecond;

            var totalDps = _assistantService.TotalDps;
            if (AwaySeconds < MinAwaySeconds || totalDps <= 0f)
            {
                return;
            }

            var cappedSeconds = Math.Min(AwaySeconds, CapSeconds);
            PendingCoins = MathfRound.Round(totalDps * cappedSeconds * Efficiency);
        }
    }
}
