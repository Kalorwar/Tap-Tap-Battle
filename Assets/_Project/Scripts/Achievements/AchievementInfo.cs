namespace _Project.Scripts.Achievements
{
    /// <summary>
    /// Payload for AchievementUnlockedSignal.
    /// </summary>
    public readonly struct AchievementInfo
    {
        public readonly string Id;
        public readonly string DisplayName;
        public readonly int DiamondReward;

        public AchievementInfo(string id, string displayName, int diamondReward)
        {
            Id = id;
            DisplayName = displayName;
            DiamondReward = diamondReward;
        }
    }
}
