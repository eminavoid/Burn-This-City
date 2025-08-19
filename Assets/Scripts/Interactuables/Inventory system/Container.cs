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

    public void LootAll()
    {
        if (contents == null || contents.Count == 0) return;
        InventoryManager.Instance.AddMany(contents);
        contents.Clear();

        InventoryUI.RefreshIfAnyOpen();
    }
}
