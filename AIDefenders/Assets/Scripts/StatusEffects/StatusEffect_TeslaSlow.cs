public class StatusEffect_TeslaSlow : StatusEffect
{
    public float speedScale = 0.7f;

    public StatusEffect_TeslaSlow(float durationSeconds, float remainingSpeedScale)
    {
        effectId = "tesla_slow";
        duration = durationSeconds;
        speedScale = remainingSpeedScale;
        allowMultiple = false;
        refreshDuration = true;
    }

    public override float ModifyMoveSpeed(float speed)
    {
        return speed * speedScale;
    }
}
