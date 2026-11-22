using UnityEngine;
using UnityEngine.Serialization;

public class Target_Friendly : Target_Base
{
    [Header("Pass Reward")]
    public int passFunds = 15;

    [Header("Friendly Fire")]
    [FormerlySerializedAs("killReward")]
    public int friendlyFireFunds = 10;

    protected override void OnKilled(WaveDirector director) { ApplyAllegianceOutcome(director, true); }
    protected override void OnReachedGoal(WaveDirector director) { ApplyAllegianceOutcome(director, false); }
}
