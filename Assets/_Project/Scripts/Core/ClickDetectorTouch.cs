using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.EnhancedTouch;
using Touch = UnityEngine.InputSystem.EnhancedTouch.Touch;
using TouchPhase = UnityEngine.InputSystem.TouchPhase;

namespace _Project.Scripts.Core
{
    public struct TapEvent
    {
    }

    public class ClickDetectorTouch : MonoBehaviour
    {
        [SerializeField] private bool _debugLog;

        private readonly List<RaycastResult> _raycastResults = new(8);
        private PointerEventData _eventData;

        private void Awake()
        {
            EnhancedTouchSupport.Enable();
            _eventData = new PointerEventData(EventSystem.current);

            this.UpdateAsObservable()
                .Where(_ => Touch.activeTouches.Count > 0)
                .Select(_ => Touch.activeTouches[0])
                .Where(touch => touch.phase == TouchPhase.Began)
                .Select(touch => new TapEvent())
                .Where(tap => !IsPointerOverUI())
                .Subscribe(tap =>
                {
                    if (_debugLog)
                    {
                        Debug.Log("[Tap]");
                    }

                    MessageBroker.Default.Publish(tap);
                })
                .AddTo(this);
        }

        private bool IsPointerOverUI()
        {
            if (EventSystem.current == null)
            {
                return false;
            }

            _raycastResults.Clear();

            EventSystem.current.RaycastAll(_eventData, _raycastResults);

            for (var i = 0; i < _raycastResults.Count; i++)
            {
                if (_raycastResults[i].gameObject.GetComponent<RectTransform>() != null)
                {
                    return true;
                }
            }

            return false;
        }
    }
}