using UnityEngine;
using System.Collections.Generic;

public class Container : MonoBehaviour, IInteractable
{
    [Header("Contenidos del cofre")]
    public List<ItemAmount> contents = new List<ItemAmount>();

    public string InteractionPrompt => "Abrir";
    public bool CanInteract(StatManager stats) => true;

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
        if (index < 0 || index >= contents.Count) return;

        var ia = contents[index];
        if (ia?.item == null || ia.amount <= 0) return;

        InventoryManager.Instance.Add(ia.item, ia.amount);
        contents.RemoveAt(index);

        // refrescar si hay UI abierta
        InventoryUI.RefreshIfAnyOpen();
    }
    public bool LootPartial(int index, int amount)
    {
        if (contents == null || index < 0 || index >= contents.Count) return false;

        var ia = contents[index];
        if (ia == null || ia.item == null || ia.amount <= 0) return false;

        int take = Mathf.Clamp(amount, 1, ia.amount);

        InventoryManager.Instance.Add(ia.item, take);

        ia.amount -= take;

        if (ia.amount <= 0)
        {
            contents.RemoveAt(index);
        }
        else
        {
            contents[index] = ia;
        }

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.RefreshContainer(this);
            InventoryUI.RefreshIfAnyOpen();
        }
        return true;
    }
    public void LootAll()
    {
        if (contents == null || contents.Count == 0) return;
        InventoryManager.Instance.AddMany(contents);
        contents.Clear();

        InventoryUI.RefreshIfAnyOpen();
    }
    public void AddItem(InventoryItem item, int amount)
    {
        if (item == null || amount <= 0) return;
        // Merge stacks of same item (simple: same SO reference)
        for (int i = 0; i < contents.Count; i++)
        {
            if (contents[i]?.item == item)
            {
                contents[i].amount += amount;
                return;
            }
        }
        contents.Add(new ItemAmount { item = item, amount = amount });
    }

    public bool TryReceiveFromInventory(InventoryItem item, int amount)
    {
        if (InventoryManager.Instance == null) return false;
        if (!InventoryManager.Instance.Remove(item, amount)) return false;

        AddItem(item, amount);
        // Refresh both sides if split open
        InventoryUI.RefreshIfAnyOpen();
        return true;
    }
    
}
