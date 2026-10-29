using _Project.Scripts.Events;
using Assets.PixelFantasy.Common.Scripts;
using UnityEngine;

namespace _Project.Scripts.Core
{
    public class PlayerTapHandler : MonoBehaviour
    {
        private UnitAnimation _unitAnimation;

        private void Awake()
        {
            _unitAnimation = new UnitAnimation(GetComponent<Creature>());
        }

        private void OnEnable()
        {
            TapEvent.OnTapped += OnTapped;
        }

        private void OnDisable()
        {
            TapEvent.OnTapped -= OnTapped;
        }

        private void OnTapped()
        {
            _unitAnimation.Slash();
        }
    }

    public class UnitAnimation
    {
        private static readonly int Slash1 = Animator.StringToHash("Slash");
        private readonly Creature _character;

        public UnitAnimation(Creature character)
        {
            _character = character;
        }

        public void Slash()
        {
            _character.Animator.SetTrigger(Slash1);
        }
    }
}
