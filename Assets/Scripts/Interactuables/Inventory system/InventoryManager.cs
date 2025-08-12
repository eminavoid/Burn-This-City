using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class InventoryEntry
{
    public InventoryItem item;
    public int amount;
}

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Contenido actual del inventario")]
    [SerializeField] private List<InventoryEntry> inventoryEntries = new List<InventoryEntry>();

    private Dictionary<InventoryItem, int> counts = new Dictionary<InventoryItem, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        SyncDictFromList();
    }

#if UNITY_EDITOR
    private void OnValidate()
    {
        SyncDictFromList();
    }
#endif

    private void SyncDictFromList()
    {
        counts.Clear();
        foreach (var entry in inventoryEntries)
        {
            if (entry.item != null)
                counts[entry.item] = Mathf.Max(0, entry.amount);
        }
    }

    private void SyncListFromDict()
    {
        inventoryEntries.Clear();
        foreach (var kvp in counts)
        {
            inventoryEntries.Add(new InventoryEntry { item = kvp.Key, amount = kvp.Value });
        }
    }

    public int GetCount(InventoryItem item)
    {
        return (item != null && counts.TryGetValue(item, out var v)) ? v : 0;
    }

    public void Add(InventoryItem item, int amount)
    {
        if (item == null || amount <= 0) return;
        counts[item] = GetCount(item) + amount;
        SyncListFromDict();
    }

    public bool CanConsume(List<ItemAmount> list)
    {
        if (list == null || list.Count == 0) return true;
        foreach (var ia in list)
        {
            if (ia.item == null || GetCount(ia.item) < ia.amount) return false;
        }
        return true;
    }

    public bool Consume(List<ItemAmount> list)
    {
        if (!CanConsume(list)) return false;
        foreach (var ia in list)
        {
            counts[ia.item] = GetCount(ia.item) - ia.amount;
            if (counts[ia.item] <= 0) counts.Remove(ia.item);
        }
        SyncListFromDict();
        return true;
    }

    public void AddMany(List<ItemAmount> list)
    {
        if (list == null) return;
        foreach (var ia in list) Add(ia.item, ia.amount);
    }
}
