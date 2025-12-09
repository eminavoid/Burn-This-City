using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[DisallowMultipleComponent]
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class ItemSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    // Origen MÓDULO
    private int srcModule = -1, srcSlot = -1;

    // Origen CONTENEDOR
    private Container sourceContainer;
    private int sourceIndex = -1;

    // Compat inventario agregado (si no lo usás más, podés borrar este bloque)
    private bool legacyInventoryAggregate = false;

    private InventoryItem item;
    private int amount;

    public void InitFromInventoryModule(int moduleIndex, int slotIndex, InventoryItem item, int amount, Image icon)
    {
        srcModule = moduleIndex; srcSlot = slotIndex;
        this.item = item; this.amount = amount; this.iconImage = icon;
        sourceContainer = null; sourceIndex = -1; legacyInventoryAggregate = false;
    }

    public void InitFromContainer(Container container, int index, InventoryItem item, int amount, Image icon)
    {
        sourceContainer = container; sourceIndex = index;
        this.item = item; this.amount = amount; this.iconImage = icon;
        srcModule = -1; srcSlot = -1; legacyInventoryAggregate = false;
    }

    // (Compat) inventario agregado
    public void InitFromInventory(InventoryItem item, int amount, Image icon)
    {
        srcModule = -1; srcSlot = -1;
        this.item = item; this.amount = amount; this.iconImage = icon;
        sourceContainer = null; sourceIndex = -1; legacyInventoryAggregate = true;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (item == null || amount <= 0) return;

        InventoryUI.Instance?.HideActiveConsumeButton();

        var cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; cg.alpha = 0.6f;

        Sprite icon = (iconImage && iconImage.sprite != null) ? iconImage.sprite : (item ? item.icon : null);

        DragAndDropController.Instance?.Begin(icon, dropTarget =>
        {
            var slot = dropTarget ? dropTarget.GetComponentInParent<SlotDropTarget>() : null; // slot exacto
            var gridDrop = dropTarget ? dropTarget.GetComponentInParent<ModuleGridDropTarget>() : null; // área módulo
            var contDrop = dropTarget ? dropTarget.GetComponentInParent<ContainerDropTarget>() : null; // área contenedor

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            // ======== CONTAINER -> MÓDULO ========

            void HandleContainerToInventory(int targetModuleIdx, int targetSlotIdx)
            {
                InventoryManager.Instance.PeekSlot(targetModuleIdx, targetSlotIdx, out var destItem, out var destAmount);

                int moveAmount = ctrl ? 1 : amount;

                if (destItem == null || (destItem == item && item.stackable))
                {
                    bool placed = InventoryManager.Instance.PlaceIntoSlot(
                        targetModuleIdx, targetSlotIdx, item, moveAmount, allowMerge: true, allowSwap: false
                    );
                    if (placed) sourceContainer.RemoveAt(sourceIndex, moveAmount);
                }
                else
                {

                    int removedAmt = InventoryManager.Instance.RemoveFromSlot(targetModuleIdx, targetSlotIdx, destAmount);

                    if (removedAmt > 0)
                    {
                        InventoryManager.Instance.PlaceIntoSlot(targetModuleIdx, targetSlotIdx, item, moveAmount);


                        if (moveAmount >= amount)
                        {
                            sourceContainer.SwapItemAt(sourceIndex, destItem, removedAmt);
                        }
                        else
                        {
                            sourceContainer.RemoveAt(sourceIndex, moveAmount);
                            int returned = sourceContainer.AddItemDirect(destItem, removedAmt);

                            if (returned > 0) Debug.LogWarning("El cofre estaba lleno, se perdieron items en el swap parcial.");
                        }
                    }
                }
            }

            // a) Soltando sobre SLOT exacto
            if (sourceContainer != null && slot != null)
            {
                HandleContainerToInventory(slot.moduleIndex, slot.slotIndex);
                return;
            }

            // b) Soltando sobre ÁREA del módulo (busca el más cercano)
            if (sourceContainer != null && gridDrop != null)
            {
                int nearest = gridDrop.FindNearestSlotIndex(ModuleGridDropTarget.LastDropScreenPos);
                if (nearest >= 0)
                {
                    HandleContainerToInventory(gridDrop.moduleIndex, nearest);
                }
                return;
            }

            // ======== MÓDULO -> MÓDULO ========

            // a) soltando sobre SLOT exacto
            if (srcModule >= 0 && srcSlot >= 0 && slot != null)
            {
                int move = ctrl ? 1 : amount;
                InventoryManager.Instance.Move(srcModule, srcSlot, slot.moduleIndex, slot.slotIndex, move);
                return;
            }

            // b) soltando sobre ÁREA del módulo (nearest)
            if (srcModule >= 0 && srcSlot >= 0 && gridDrop != null)
            {
                int move = ctrl ? 1 : amount;
                int nearest = gridDrop.FindNearestSlotIndex(ModuleGridDropTarget.LastDropScreenPos);
                if (nearest >= 0)
                    InventoryManager.Instance.Move(srcModule, srcSlot, gridDrop.moduleIndex, nearest, move);
                return;
            }

            // ======== MÓDULO -> CONTENEDOR ========

            if (srcModule >= 0 && srcSlot >= 0 && contDrop != null)
            {
                var container = contDrop.boundContainer;
                Debug.Log($"[DnD] Module({srcModule}:{srcSlot}) -> Container '{(container ? container.name : "null")}' via {contDrop.name}");

                if (container != null)
                {
                    // 1) Resolver el item actual del slot (por si 'item' del drag quedó null/stale)
                    InventoryItem srcItem = item;
                    int slotAmount = amount;

                    if ((srcItem == null || slotAmount <= 0) && InventoryManager.Instance != null)
                    {
                        InventoryManager.Instance.PeekSlot(srcModule, srcSlot, out srcItem, out slotAmount);
                    }
                    if (srcItem == null)
                    {
                        Debug.LogWarning("[DnD] PeekSlot no encontró item en el origen; abortando.");
                        return;
                    }

                    // 2) Determinar cuánto mover (Ctrl=1). Si 'amount' del drag es inválido, usar el del slot.
                    int move = ctrl ? 1 : (amount > 0 ? amount : Mathf.Max(1, slotAmount));

                    // 3) Remover del slot y agregar al contenedor con el ITEM correcto
                    int taken = InventoryManager.Instance.RemoveFromSlot(srcModule, srcSlot, move);
                    Debug.Log($"[DnD] RemoveFromSlot took={taken}");

                    if (taken > 0)
                    {
                        int rejected = container.AddItemDirect(srcItem, taken);

                        if (rejected > 0)
                        {
                            // Devolvemos lo que el cofre escupió
                            InventoryManager.Instance.Add(srcItem, rejected);

                            // Feedback visual (opcional)
                            Debug.Log($"El cofre rechazó {rejected} items. Devueltos al inventario.");
                        }

                        if (InventoryUI.Instance && InventoryUI.Instance.ContainerGridUI)
                            InventoryUI.Instance.ContainerGridUI.Refresh();
                    }
                    else
                    {
                        Debug.LogWarning("[DnD] RemoveFromSlot devolvió 0; nada que agregar.");
                    }
                }
                else
                {
                    Debug.LogError("[DnD] contDrop.boundContainer == null");
                }
                return;
            }


            // ======== (Compat) inventario agregado -> contenedor (si aún lo usás) ========
            if (legacyInventoryAggregate && contDrop != null)
            {
                var container = InventoryUI.Instance ? InventoryUI.Instance.CurrentContainer : null;
                if (container != null)
                {
                    int move = ctrl ? 1 : amount;
                    container.TryReceiveFromInventory(item, move);
                }
                return;
            }
        });
    }

    public void OnDrag(PointerEventData e) => DragAndDropController.Instance?.Move(e.position);

    public void OnEndDrag(PointerEventData e)
    {
        var cg = GetComponent<CanvasGroup>(); if (cg) { cg.blocksRaycasts = true; cg.alpha = 1f; }
        DragAndDropController.Instance?.Cancel();
    }
}
