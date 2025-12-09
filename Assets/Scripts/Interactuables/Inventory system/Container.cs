using UnityEngine;
using System.Collections.Generic;

public class Container : MonoBehaviour, IInteractable
{
    [Header("Configuración")]
    [Tooltip("Distancia máxima a la que el jugador puede alejarse antes de que se cierre la UI.")]
    public float maxDistanceToClose = 3.0f;
    public int capacity = 6;

    [Header("Save System")]
    [Tooltip("ID Único para guardar el estado. Haz clic derecho en el componente -> Generate ID")]
    public string containerID;

    [Header("Contenidos del cofre")]
    public List<ItemAmount> contents = new List<ItemAmount>();

    [SerializeField] public GameObject icon;
    
    public string InteractionPrompt => "Abrir";
    public bool CanInteract(StatManager stats) => true;

    public event System.Action OnChanged;
    private void NotifyChanged() => OnChanged?.Invoke();

    public void Interact(StatManager stats)
    {
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
            if (InventoryUI.Instance != null && InventoryUI.Instance.CurrentContainer == this)
            {
                InventoryUI.Instance.CloseAll();
            }
        }
    }

    public void LootOne(int index)
    {
        if (contents == null || index < 0 || index >= contents.Count) return;
        var ia = contents[index];
        if (ia == null || ia.item == null || ia.amount <= 0) return;

        int leftOver = InventoryManager.Instance.Add(ia.item, ia.amount);

        if (leftOver == 0)
        {
            contents.RemoveAt(index);
        }
        else if (leftOver < ia.amount)
        {
            ia.amount = leftOver;
            contents[index] = ia;       

        }
        NotifyChanged();
    }
    public bool LootPartial(int index, int amount)
    {
        if (contents == null || index < 0 || index >= contents.Count) return false;
        var ia = contents[index];
        if (ia == null || ia.item == null || ia.amount <= 0) return false;

        int wantToTake = Mathf.Clamp(amount, 1, ia.amount);

        int leftOver = InventoryManager.Instance.Add(ia.item, wantToTake);

        int actuallyTaken = wantToTake - leftOver;

        if (actuallyTaken > 0)
        {
            ia.amount -= actuallyTaken;

            if (ia.amount <= 0) contents.RemoveAt(index);
            else contents[index] = ia;

            NotifyChanged();
            return true;
        }

        return false;       
    }
    public void LootAll()
    {
        if (contents == null || contents.Count == 0) return;

        for (int i = contents.Count - 1; i >= 0; i--)
        {
            LootOne(i);
        }

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
        AddItem(item, amount);     
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
    public int AddItemDirect(InventoryItem item, int amount)
    {
        if (contents == null) contents = new List<ItemAmount>();
        if (item == null || amount <= 0) return amount;

        int remaining = amount;

        if (item.stackable)      
        {
            for (int i = 0; i < contents.Count; i++)
            {
                if (contents[i].item == item)
                {
                    int spaceInStack = item.maxStack - contents[i].amount;

                    if (spaceInStack > 0)
                    {
                        int toAdd = Mathf.Min(remaining, spaceInStack);

                        contents[i].amount += toAdd;
                        remaining -= toAdd;

                        Debug.Log($"[Container] Merged {toAdd} of {item.name}. Remaining: {remaining}");

                        if (remaining <= 0)
                        {
                            NotifyChanged();
                            return 0;   
                        }
                    }
                }
            }
        }

        while (remaining > 0)
        {
            if (contents.Count < capacity)
            {
                int amountForNewSlot = item.stackable ? Mathf.Min(remaining, item.maxStack) : 1;

                contents.Add(new ItemAmount { item = item, amount = amountForNewSlot });
                remaining -= amountForNewSlot;
            }
            else
            {
                Debug.LogWarning("[Container] Lleno. Devolviendo sobrante.");
                NotifyChanged();        
                return remaining;
            }
        }

        NotifyChanged();
        return 0;
    }
    public void SwapItemAt(int index, InventoryItem newItem, int newAmount)
    {
        if (contents != null && index >= 0 && index < contents.Count)
        {
            contents[index] = new ItemAmount { item = newItem, amount = newAmount };
            NotifyChanged();              
        }
        else
        {
            Debug.LogWarning($"[Container] Intento de Swap inválido en índice {index}. Agregando como nuevo.");
            AddItemDirect(newItem, newAmount);
        }
    }

#if UNITY_EDITOR
    private void Reset()
    {
        if (string.IsNullOrEmpty(containerID))
        {
            GenerateID();
        }
    }

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(containerID))
        {
            GenerateID();
        }
    }

    [ContextMenu("Generate ID")]
    private void GenerateID()
    {
        containerID = System.Guid.NewGuid().ToString();

        UnityEditor.EditorUtility.SetDirty(this);

        if (!Application.isPlaying)
        {
            UnityEditor.SceneManagement.EditorSceneManager.MarkSceneDirty(gameObject.scene);
        }

        Debug.Log($"[Container] Generated ID for {gameObject.name}: {containerID}");
    }
#endif
}
