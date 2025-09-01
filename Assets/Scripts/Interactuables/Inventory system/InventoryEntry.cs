using UnityEngine;

[System.Serializable]
public class InventoryEntry
{
    public InventoryItem item;
    public int amount;

    public InventoryEntry(InventoryItem item, int amount)
    {
        this.item = item;
        this.amount = amount;
    }
}
