using UnityEngine;
public enum ItemCategory { Quest, Survival, Valuables, Tools , DEFAULT }

[CreateAssetMenu(fileName = "NewItem", menuName = "Inventory/Item")]
public class InventoryItem : ScriptableObject
{
    [Header("Visual")]
    public string displayName;
    public Sprite icon;

    [Header("Stacking")]
    public bool stackable = true;
    public int maxStack = 99;

    [Header("Category")]
    public ItemCategory category = ItemCategory.DEFAULT;
}
