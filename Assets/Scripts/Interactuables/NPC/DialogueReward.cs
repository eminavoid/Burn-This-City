using UnityEngine;

[System.Serializable]
public class DialogueReward
{
    public enum RewardType
    {
        Stat,
        Health,
        Sanity
    }

    public RewardType rewardType = RewardType.Stat;
    public StatManager.StatType statType;
    public int amount;
}
