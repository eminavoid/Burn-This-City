using UnityEngine;
using System.Collections.Generic;

public class Container : MonoBehaviour, IInteractable
{
    [Header("Contenidos del cofre")]
    public List<ItemAmount> contents = new List<ItemAmount>();

    [SerializeField] public GameObject icon;
    
    public string InteractionPrompt => "Abrir";
    public bool CanInteract(StatManager stats) => true;

    public event System.Action OnChanged;
    private void NotifyChanged() => OnChanged?.Invoke();

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

    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            icon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            icon.SetActive(false);
        }
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

        // ¿ya había un stack del mismo item?
        for (int i = 0; i < contents.Count; i++)
        {
            if (contents[i]?.item == item)
            {
                contents[i].amount += amount;
                Debug.Log($"[Container] AddItem MERGE '{item.name}' +{amount} -> stack={contents[i].amount}; stacks={contents.Count}");
                OnChanged?.Invoke();
                return;
            }
        }

        contents.Add(new ItemAmount { item = item, amount = amount });
        Debug.Log($"[Container] AddItem NEW '{item.name}' +{amount}; stacks={contents.Count}");
        OnChanged?.Invoke();
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

        NotifyChanged();
        return true;
    }
    public void AddItemDirect(InventoryItem item, int amount)
    {
        if (contents == null) contents = new List<ItemAmount>();
        if (item == null || amount <= 0) { Debug.LogWarning("[Container] AddItemDirect ignorado (item null o amount <= 0)"); return; }

        int idx = contents.FindIndex(x => x != null && x.item == item);
        if (idx >= 0)
        {
            contents[idx].amount += amount;
            Debug.Log($"[Container] AddItemDirect MERGE '{item.name}' +{amount} -> stack={contents[idx].amount}; stacks={contents.Count}");
        }
        else
        {
            contents.Add(new ItemAmount { item = item, amount = amount });
            Debug.Log($"[Container] AddItemDirect NEW '{item.name}' +{amount}; stacks={contents.Count}");
        }

        NotifyChanged();
    }
}
