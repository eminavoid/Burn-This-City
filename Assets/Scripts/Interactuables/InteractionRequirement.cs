using System;

[Serializable]
public class InteractionRequirement
{
    public StatManager.StatType statType;
    public enum ComparisonType
    {
        GreaterThan,
        LessThan,
        EqualTo,
        GreaterOrEqualTo,
        LessOrEqualTo
    }
    public ComparisonType comparisonType;
    public int requiredAmount;
}