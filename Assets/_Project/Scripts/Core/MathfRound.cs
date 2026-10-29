using UnityEngine;

namespace _Project.Scripts.Core
{
    public struct MathfRound
    {
        public static float Round(float value)
        {
            return Mathf.Round(value * 10f) / 10f;
        }
    }
}