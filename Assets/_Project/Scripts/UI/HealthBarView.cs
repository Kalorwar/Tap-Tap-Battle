using UnityEngine.UI;

namespace _Project.Scripts.UI
{
    public class HealthBarView
    {
        private readonly Slider _slider;

        public HealthBarView(Slider slider)
        {
            _slider = slider;
        }

        public void SetPercent(float percent)
        {
            _slider.value = percent;
        }

        public void ResetToFull()
        {
            _slider.value = 1f;
        }
    }
}