namespace _Project.Scripts.UI
{
    public readonly struct DamageChunkAnimationSettings
    {
        public readonly float FlashDuration;
        public readonly float FadeDuration;
        public readonly float MoveDistance;

        public DamageChunkAnimationSettings(float flashDuration, float fadeDuration, float moveDistance)
        {
            FlashDuration = flashDuration;
            FadeDuration = fadeDuration;
            MoveDistance = moveDistance;
        }
    }
}