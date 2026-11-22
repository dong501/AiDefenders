using UnityEngine;
using UnityEngine.Serialization;

public class Target_Enemy : Target_Base
{
    [Header("Enemy Reward")]
    [FormerlySerializedAs("killReward")]
    public int killFunds = 10;

    [Header("Base")]
    public float baseDamage = 1f;

    protected override void OnKilled(WaveDirector director) { ApplyAllegianceOutcome(director, true); }
    protected override void OnReachedGoal(WaveDirector director) { ApplyAllegianceOutcome(director, false); }
}
