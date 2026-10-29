using UnityEngine;
using Zenject;

namespace _Project.Scripts.Core
{
    /// <summary>
    /// Keeps the offline-progress timestamp fresh: on quit, on focus loss and periodically.
    /// Must live on an active scene object; injects the service via Zenject.
    /// </summary>
    public class MetaSessionTracker : MonoBehaviour
    {
        private const float TouchIntervalSeconds = 60f;

        private OfflineProgressService _offline;
        private float _timer;

        [Inject]
        private void Construct(OfflineProgressService offline)
        {
            _offline = offline;
        }

        private void Update()
        {
            _timer += UnityEngine.Time.unscaledDeltaTime;
            if (_timer < TouchIntervalSeconds)
            {
                return;
            }

            _timer = 0f;
            Touch();
        }

        private void OnApplicationPause(bool pauseStatus)
        {
            if (pauseStatus)
            {
                Touch();
            }
        }

        private void OnApplicationQuit()
        {
            Touch();
        }

        private void Touch()
        {
            if (_offline != null)
            {
                _offline.TouchLastSeen();
            }
        }
    }
}
