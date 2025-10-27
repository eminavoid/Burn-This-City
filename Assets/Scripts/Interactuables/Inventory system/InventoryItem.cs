using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Visual")]
    public string displayName;
    public Sprite icon;

    [Header("Stacking")]
    public bool stackable = true;
    public int maxStack = 99;

    [Header("Consumable Settings")]
    [Tooltip("If true, clicking this item in the inventory shows a 'Consume' button.")]
    public bool isConsumable = false;

    [Tooltip("List of effects applied when consumed (e.g. heal HP, restore Sanity).")]
    public List<ConsumableEffect> consumableEffects = new();
}

[System.Serializable]
public class ConsumableEffect
{
    [Tooltip("Which attribute this consumable affects.")]
    public ConsumableType type = ConsumableType.Health;

    [Tooltip("Which stat to modify, if type is 'Stat'.")]
    public StatManager.StatType statType;

    [Tooltip("How much to heal or restore. Can be negative to damage.")]
    public float amount = 0f;
}

public enum ConsumableType
{
    Health,
    Sanity,
    Stat
}