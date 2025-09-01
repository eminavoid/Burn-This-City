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

    public event Action OnInventoryChanged;

    [Header("Contenido actual del inventario")]
    [SerializeField] private List<InventoryEntry> inventoryEntries = new List<InventoryEntry>();
    private Dictionary<InventoryItem, int> counts = new Dictionary<InventoryItem, int>();

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        SyncDictFromList();
        OnInventoryChanged?.Invoke();
    }

#if UNITY_EDITOR
    private void OnValidate() { SyncDictFromList(); OnInventoryChanged?.Invoke(); }
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
            inventoryEntries.Add(new InventoryEntry { item = kvp.Key, amount = kvp.Value });
    }

    public int GetCount(InventoryItem item)
        => (item != null && counts.TryGetValue(item, out var v)) ? v : 0;

    public void Add(InventoryItem item, int amount)
    {
        if (item == null || amount <= 0) return;
        counts[item] = GetCount(item) + amount;
        SyncListFromDict();
        OnInventoryChanged?.Invoke();
    }

    public void AddMany(List<ItemAmount> list)
    {
        if (list == null || list.Count == 0) return;

        // Agregamos todo primero sin spamear eventos
        bool changed = false;
        foreach (var ia in list)
        {
            if (ia == null || ia.item == null || ia.amount <= 0) continue;
            counts[ia.item] = GetCount(ia.item) + ia.amount;
            changed = true;
        }

        if (changed)
        {
            SyncListFromDict();
            OnInventoryChanged?.Invoke(); // un solo evento por lote
        }
    }

    public bool CanConsume(List<ItemAmount> list)
    {
        if (list == null || list.Count == 0) return true;
        foreach (var ia in list)
            if (ia.item == null || GetCount(ia.item) < ia.amount) return false;
        return true;
    }
    public bool Remove(InventoryItem item, int amount)
    {
        if (item == null || amount <= 0) return false;
        int have = GetCount(item);
        if (have < amount) return false;

        counts[item] = have - amount;
        if (counts[item] <= 0) counts.Remove(item);

        SyncListFromDict();
        OnInventoryChanged?.Invoke();
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
        OnInventoryChanged?.Invoke();
        return true;
    }

    // —— Consultas para UI ——
    public List<InventoryEntry> GetEntriesByCategory(ItemCategory cat)
    {
        var result = new List<InventoryEntry>();
        foreach (var kvp in counts)
            if (kvp.Key != null && kvp.Key.category == cat)
                result.Add(new InventoryEntry { item = kvp.Key, amount = kvp.Value });
        return result;
    }

    public List<InventoryEntry> GetAllEntries()
    {
        var result = new List<InventoryEntry>();
        foreach (var kvp in counts)
            if (kvp.Key != null)
                result.Add(new InventoryEntry { item = kvp.Key, amount = kvp.Value });
        return result;
    }
}

