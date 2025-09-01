using UnityEngine;
using System.Collections.Generic;

public class Container : MonoBehaviour, IInteractable
{
    [Header("Contenidos del cofre")]
    public List<ItemAmount> contents = new List<ItemAmount>();

    public string InteractionPrompt => "Abrir";
    public bool CanInteract(StatManager stats) => true;

    public event System.Action OnChanged;
    private void NotifyChanged()
    {
        OnChanged?.Invoke();
    }
    public void Interact(StatManager stats)
    {
        // Log para ver qué tiene el cofre al momento de abrir
        Debug.Log($"[Container] Abrir split. Items en contents: {contents?.Count ?? -1}");
        if (contents != null)
        {
            for (int i = 0; i < contents.Count; i++)
            {
                var ia = contents[i];
                Debug.Log($"  [{i}] item={(ia?.item ? ia.item.name : "NULL")} amount={ia?.amount}");
            }
        }

        InventoryUI.Instance?.OpenSplit(this);
    }

    public void LootOne(int index)
    {
        if (contents == null || index < 0 || index >= contents.Count) return;
        var ia = contents[index];
        if (ia == null || ia.item == null || ia.amount <= 0) return;

        InventoryManager.Instance.Add(ia.item, ia.amount);
        contents.RemoveAt(index);
        NotifyChanged();
    }
    public bool LootPartial(int index, int amount)
    {
        if (contents == null || index < 0 || index >= contents.Count) return false;
        var ia = contents[index];
        if (ia == null || ia.item == null || ia.amount <= 0) return false;

        int take = Mathf.Clamp(amount, 1, ia.amount);
        InventoryManager.Instance.Add(ia.item, take);
        ia.amount -= take;
        if (ia.amount <= 0) contents.RemoveAt(index);
        else contents[index] = ia;

        NotifyChanged();
        return true;
    }
    public void LootAll()
    {
        if (contents == null || contents.Count == 0) return;
        InventoryManager.Instance.AddMany(contents);
        contents.Clear();
        NotifyChanged();
    }
    public void AddItem(InventoryItem item, int amount)
    {
        if (item == null || amount <= 0) return;
        for (int i = 0; i < contents.Count; i++)
        {
            if (contents[i]?.item == item)
            {
                contents[i].amount += amount;
                NotifyChanged();
                return;
            }
        }
        contents.Add(new ItemAmount { item = item, amount = amount });
        NotifyChanged();
    }

    public bool TryReceiveFromInventory(InventoryItem item, int amount)
    {
        if (!InventoryManager.Instance.Remove(item, amount)) return false;
        AddItem(item, amount); // AddItem ya hace NotifyChanged()
        return true;
    }

    public bool RemoveAt(int index, int amount)
    {
        if (contents == null || index < 0 || index >= contents.Count) return false;
        var ia = contents[index];
        if (ia == null || ia.item == null || ia.amount <= 0) return false;

        int take = Mathf.Clamp(amount, 1, ia.amount);
        ia.amount -= take;
        if (ia.amount <= 0) contents.RemoveAt(index);
        else contents[index] = ia;

        OnChanged?.Invoke();
        return true;
    }

}
