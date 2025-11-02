using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

[Serializable] public class SlotState { public InventoryItem item; public int amount; }
[Serializable] public class ModuleState { public List<SlotState> slots = new(); }

public class InventoryManager : MonoBehaviour
{
    public static InventoryManager Instance { get; private set; }

    [Header("Módulos (M0..M3)")]
    public List<InventoryModuleDef> moduleDefs = new();  // asigná los 4 defs en el inspector

    [SerializeField] private List<ModuleState> modules = new();
    public event Action OnInventoryChanged;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; DontDestroyOnLoad(gameObject);
        InitModules();
        OnInventoryChanged?.Invoke();
    }

    public void InitModules()
    {
        modules.Clear();
        foreach (var def in moduleDefs)
        {
            var m = new ModuleState();
            for (int i = 0; i < def.Capacity; i++) m.slots.Add(new SlotState());
            modules.Add(m);
        }
    }

    public IReadOnlyList<ModuleState> Modules => modules;
    public ModuleState GetModule(int i) => (i >= 0 && i < modules.Count) ? modules[i] : null;

    // --- API clásica ---
    public int GetCount(InventoryItem item)
    {
        if (!item) return 0; int total = 0;
        foreach (var m in modules) foreach (var s in m.slots) if (s.item == item) total += s.amount;
        return total;
    }

    public void Add(InventoryItem item, int amount)
    {
        if (!item || amount <= 0) return;
        int left = amount;

        // apilar
        foreach (var m in modules)
            for (int i = 0; i < m.slots.Count && left > 0; i++)
                if (m.slots[i].item == item && item.stackable && m.slots[i].amount < item.maxStack)
                { int can = Mathf.Min(item.maxStack - m.slots[i].amount, left); m.slots[i].amount += can; left -= can; }

        // slots vacíos
        foreach (var m in modules)
            for (int i = 0; i < m.slots.Count && left > 0; i++)
                if (m.slots[i].item == null)
                { int put = item.stackable ? Mathf.Min(item.maxStack, left) : 1; m.slots[i].item = item; m.slots[i].amount = put; left -= put; }

        OnInventoryChanged?.Invoke();
    }

    public bool Remove(InventoryItem item, int amount)
    {
        if (!item || amount <= 0) return false;
        int need = amount;
        foreach (var m in modules)
        {
            for (int i = 0; i < m.slots.Count && need > 0; i++)
            {
                var s = m.slots[i]; if (s.item != item) continue;
                int take = Mathf.Min(s.amount, need);
                s.amount -= take; if (s.amount <= 0) { s.item = null; s.amount = 0; }
                need -= take;
            }
            if (need <= 0) break;
        }
        if (need == 0) { OnInventoryChanged?.Invoke(); return true; }
        return false;
    }

    // mover entre módulos/slots (para DnD interno)
    public bool Move(int fromModule, int fromSlot, int toModule, int toSlot, int amount)
    {
        var srcM = GetModule(fromModule); var dstM = GetModule(toModule);
        if (srcM == null || dstM == null) return false;
        if (fromSlot < 0 || fromSlot >= srcM.slots.Count) return false;
        if (toSlot < 0 || toSlot >= dstM.slots.Count) return false;

        var src = srcM.slots[fromSlot]; var dst = dstM.slots[toSlot];
        if (src.item == null || src.amount <= 0) return false;

        int move = Mathf.Clamp(amount, 1, src.amount);

        if (dst.item == null)
        {
            dst.item = src.item; dst.amount = move;
            src.amount -= move; if (src.amount <= 0) { src.item = null; src.amount = 0; }
        }
        else if (dst.item == src.item && src.item.stackable)
        {
            int can = Mathf.Min(src.item.maxStack - dst.amount, move);
            dst.amount += can; src.amount -= can; if (src.amount <= 0) { src.item = null; src.amount = 0; }
        }
        else // swap
        {
            (dst.item, src.item) = (src.item, dst.item);
            (dst.amount, src.amount) = (src.amount, dst.amount);
        }

        OnInventoryChanged?.Invoke();
        return true;
    }
    // === API unificada útil para gameplay ===
    public bool Has(InventoryItem item, int amount = 1)
    {
        return GetCount(item) >= Mathf.Max(1, amount);
    }

    public bool TryConsume(InventoryItem item, int amount = 1)
    {
        if (!item || amount <= 0) return false;
        int need = amount;

        // Recorre todos los módulos y descuenta
        foreach (var mod in modules)
        {
            for (int i = 0; i < mod.slots.Count && need > 0; i++)
            {
                var s = mod.slots[i];
                if (s.item != item) continue;
                int take = Mathf.Min(s.amount, need);
                s.amount -= take;
                if (s.amount <= 0) { s.item = null; s.amount = 0; }
                need -= take;
            }
            if (need <= 0) break;
        }

        if (need == 0) { OnInventoryChanged?.Invoke(); return true; }
        return false;
    }

    // === SHIMS de COMPATIBILIDAD con código viejo ===

    // ========== REQS MÚLTIPLES (p.ej. recetas / diálogo) ==========

// ¿Alcanza para TODOS los requisitos? (no consume)
public bool CanConsume(System.Collections.Generic.IEnumerable<ItemAmount> requirements)
{
    if (requirements == null) return true;

    // Sumamos por item en caso de requisitos repetidos
    var need = new System.Collections.Generic.Dictionary<InventoryItem, int>();
    foreach (var ia in requirements)
    {
        if (ia == null || ia.item == null || ia.amount <= 0) continue;
        if (!need.ContainsKey(ia.item)) need[ia.item] = 0;
        need[ia.item] += ia.amount;
    }

    foreach (var kv in need)
    {
        if (GetCount(kv.Key) < kv.Value) return false;
    }
    return true;
}

// Consume TODOS los requisitos si alcanza (atómico: o todo o nada)
public bool Consume(System.Collections.Generic.IEnumerable<ItemAmount> requirements)
{
    if (!CanConsume(requirements)) return false;

    // Como sí alcanza, ahora descontamos
    foreach (var ia in requirements)
    {
        if (ia == null || ia.item == null || ia.amount <= 0) continue;
        // Reutilizamos la lógica existente de Remove que recorre módulos
        Remove(ia.item, ia.amount);
    }

    // Remove ya llama OnInventoryChanged(); si querés asegurarlo:
    OnInventoryChanged?.Invoke();
    return true;
}

// Usado por Container.LootAll / diálogos que agregan múltiples
public void AddMany(System.Collections.Generic.List<ItemAmount> items)
    {
        if (items == null) return;
        foreach (var ia in items)
        {
            if (ia?.item == null || ia.amount <= 0) continue;
            Add(ia.item, ia.amount);
        }
    }

    // Snapshot simple para UIs antiguas (Backpack plano).
    // Devuelve lista agregada {item, total} para poblar grillas simples.
    public System.Collections.Generic.List<InventoryEntry> GetAllEntries()
    {
        var dict = new System.Collections.Generic.Dictionary<InventoryItem, int>();
        foreach (var mod in modules)
        {
            foreach (var s in mod.slots)
            {
                if (s.item == null || s.amount <= 0) continue;
                if (!dict.TryGetValue(s.item, out int cur)) cur = 0;
                dict[s.item] = cur + s.amount;
            }
        }

        var list = new System.Collections.Generic.List<InventoryEntry>();
        foreach (var kv in dict)
            list.Add(new InventoryEntry(kv.Key, kv.Value));
        return list;
    }
    // ========== SHIMS INDIVIDUALES (ya los tenías, los dejo aquí por claridad) ==========
    public bool CanConsume(InventoryItem item, int amount) => Has(item, amount);
    public bool Consume(InventoryItem item, int amount) => TryConsume(item, amount);

    public bool PlaceIntoSlot(int toModule, int toSlot, InventoryItem item, int amount, bool allowMerge = true, bool allowSwap = true)
    {
        var dstM = GetModule(toModule);
        if (dstM == null || item == null || amount <= 0) return false;
        if (toSlot < 0 || toSlot >= dstM.slots.Count) return false;

        var dst = dstM.slots[toSlot];

        if (dst.item == null)
        {
            int put = item.stackable ? Mathf.Min(item.maxStack, amount) : 1;
            dst.item = item; dst.amount = put;
            OnInventoryChanged?.Invoke();
            return true;
        }
        if (allowMerge && dst.item == item && item.stackable)
        {
            int can = Mathf.Min(item.maxStack - dst.amount, amount);
            if (can > 0)
            {
                dst.amount += can;
                OnInventoryChanged?.Invoke();
                return true;
            }
            return false;
        }
        if (allowSwap)
        {
            (dst.item, item) = (item, dst.item);
            int put = dst.item.stackable ? Mathf.Min(dst.item.maxStack, amount) : 1;
            dst.amount = put;
            OnInventoryChanged?.Invoke();
            return true;
        }
        return false;
    }
    public int RemoveFromSlot(int moduleIndex, int slotIndex, int amount)
    {
        var mod = GetModule(moduleIndex);
        if (mod == null || amount <= 0) return 0;
        if (slotIndex < 0 || slotIndex >= mod.slots.Count) return 0;

        var s = mod.slots[slotIndex];
        if (s.item == null || s.amount <= 0) return 0;

        int take = Mathf.Min(amount, s.amount);
        s.amount -= take;
        if (s.amount <= 0) { s.item = null; s.amount = 0; }

        OnInventoryChanged?.Invoke();
        return take;
    }
    public bool PeekSlot(int moduleIndex, int slotIndex, out InventoryItem item, out int amount)
    {
        item = null; amount = 0;

        if (moduleIndex < 0 || moduleIndex >= modules.Count) return false;
        var mod = modules[moduleIndex];                    // tu estructura de módulo
        if (slotIndex < 0 || slotIndex >= mod.slots.Count) return false;

        var s = mod.slots[slotIndex];                      // tu tipo de slot
        if (s == null || s.item == null || s.amount <= 0) return false;

        item = s.item;
        amount = s.amount;
        return true;
    }

    public void ForceRefresh()
    {
        OnInventoryChanged?.Invoke();
    }
}

