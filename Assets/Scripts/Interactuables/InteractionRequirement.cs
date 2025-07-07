using System;
using UnityEngine;

[Serializable]
public class InteractionRequirement
{
    [Tooltip("Which stat this requirement applies to.")]
    public StatManager.StatType statType;
    public enum ComparisonType
    {
        GreaterThan,
        LessThan,
        EqualTo,
        GreaterOrEqualTo,
        LessOrEqualTo
    }
    [Tooltip("How to compare the player's stat value to the required amount.")]
    public ComparisonType comparisonType;
    [Tooltip("The value the stat must meet or compare against.")]
    public int requiredAmount;

    public bool IsMet(int statValue)
    {
        return comparisonType switch
        {
            ComparisonType.GreaterThan => statValue > requiredAmount,
            ComparisonType.LessThan => statValue < requiredAmount,
            ComparisonType.EqualTo => statValue == requiredAmount,
            ComparisonType.GreaterOrEqualTo => statValue >= requiredAmount,
            ComparisonType.LessOrEqualTo => statValue <= requiredAmount,
            _ => false
        };
    }
}