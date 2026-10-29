using UnityEngine;

namespace _Project.Scripts.Assistants
{
    [CreateAssetMenu(menuName = "Assistants/AssistantConfig")]
    public class AssistantConfig : ScriptableObject
    {
        public string Id;
        public string DisplayName;

        public float BaseDps;
        public float BaseCost;

        public float CostGrowth = 1.07f;
        public float DpsGrowth = 1.05f;

        public int UnlockLevel;
        public int UnlockBossIndex;
    }
}
