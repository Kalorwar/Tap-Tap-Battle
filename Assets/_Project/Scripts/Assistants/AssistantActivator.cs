using System;
using System.Collections.Generic;
using _Project.Scripts.Events;
using UnityEngine;
using Zenject;

namespace _Project.Scripts.Assistants
{
    /// <summary>
    /// Enables scene visuals for hired assistants (level > 0). Never disables them back.
    /// </summary>
    public class AssistantActivator : MonoBehaviour
    {
        [SerializeField] private List<AssistantBinding> _bindings = new();

        private AssistantService _assistantService;

        [Inject]
        private void Construct(AssistantService assistantService)
        {
            _assistantService = assistantService;
        }

        private void Start()
        {
            // Injection is guaranteed by now; restore visuals for previously hired assistants
            Apply();
        }

        private void OnEnable()
        {
            AssistantChangedSignal.OnAssistantChanged += Apply;
            Apply();
        }

        private void OnDisable()
        {
            AssistantChangedSignal.OnAssistantChanged -= Apply;
        }

        private void Apply()
        {
            if (_assistantService == null)
            {
                return;
            }

            foreach (var binding in _bindings)
            {
                if (binding.Config == null || binding.SceneObject == null)
                {
                    continue;
                }

                if (_assistantService.GetLevel(binding.Config.Id) > 0)
                {
                    binding.SceneObject.SetActive(true);
                }
            }
        }

        [Serializable]
        public class AssistantBinding
        {
            public AssistantConfig Config;
            public GameObject SceneObject;
        }
    }
}
