using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private Image iconImage;

    private int srcModule = -1, srcSlot = -1;
    private InventoryItem item; private int amount;

    public void InitFromInventoryModule(int moduleIndex, int slotIndex, InventoryItem item, int amount, Image icon)
    {
        srcModule = moduleIndex; srcSlot = slotIndex;
        this.item = item; this.amount = amount; this.iconImage = icon;
    }

    public void OnBeginDrag(PointerEventData e)
    {
        if (item == null || amount <= 0) return;

        var cg = GetComponent<CanvasGroup>() ?? gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false; cg.alpha = 0.6f;

        Sprite icon = (iconImage && iconImage.sprite != null) ? iconImage.sprite : (item ? item.icon : null);

        DragAndDropController.Instance?.Begin(icon, dropTarget =>
        {
            var slotTarget = dropTarget ? dropTarget.GetComponent<SlotDropTarget>() : null;
            var dz = dropTarget ? dropTarget.GetComponent<ItemDropZone>() : null; // legacy
            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            var gridDrop = dropTarget ? dropTarget.GetComponent<ModuleGridDropTarget>() : null;

            // -------- Container -> MÓDULO (drop exacto sobre un SlotDropTarget) --------
            if (sourceContainer != null && gridDrop != null)
            {
                int move = ctrl ? 1 : amount;

                // Elegimos el slot más cercano al puntero
                int nearest = gridDrop.FindNearestSlotIndex(ModuleGridDropTarget.LastDropScreenPos);
                if (nearest >= 0)
                {
                    bool placed = InventoryManager.Instance.PlaceIntoSlot(
                        gridDrop.moduleIndex, nearest, item, move, allowMerge: true, allowSwap: true
                    );
                    if (placed)
                    {
                        sourceContainer.RemoveAt(sourceIndex, move);
                    }
                }
                return;
            }

            // -------- Container -> Backpack (legacy DropZone amplio) --------
            if (sourceContainer != null && dz != null && dz.kind == ItemDropZone.DropKind.Backpack)
            {
                int move = ctrl ? 1 : amount;
                if (move < amount) sourceContainer.LootPartial(sourceIndex, move);
                else sourceContainer.LootOne(sourceIndex);
                return;
            }

            // -------- MÓDULO -> MÓDULO --------
            if (srcModule >= 0 && srcSlot >= 0 && gridDrop != null)
            {
                int move = ctrl ? 1 : amount;

                int nearest = gridDrop.FindNearestSlotIndex(ModuleGridDropTarget.LastDropScreenPos);
                if (nearest >= 0)
                {
                    InventoryManager.Instance.Move(srcModule, srcSlot, gridDrop.moduleIndex, nearest, move);
                }
                return;
            }

            // -------- Inventario agregado (legacy) -> Container --------
            if (legacyInventoryAggregate && dz != null && dz.kind == ItemDropZone.DropKind.Container)
            {
                var ui = InventoryUI.Instance; var container = ui ? ui.CurrentContainer : null;
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

    // ==== Compat: modo "inventario agregado" (no modular) ====
    public void InitFromInventory(InventoryItem item, int amount, UnityEngine.UI.Image icon)
    {
        // Modo compat: solo necesitamos saber qué item/stack se arrastra.
        srcModule = -1; srcSlot = -1;
        this.item = item; this.amount = amount; this.iconImage = icon;
        legacyInventoryAggregate = true; // flag para OnBeginDrag
    }

    // ==== Compat: arrastre desde CONTAINER ====
    public void InitFromContainer(Container container, int index, InventoryItem item, int amount, UnityEngine.UI.Image icon)
    {
        sourceContainer = container; sourceIndex = index;
        this.item = item; this.amount = amount; this.iconImage = icon;
        srcModule = -1; srcSlot = -1; legacyInventoryAggregate = false;
    }

    // Campo privado compat
    private bool legacyInventoryAggregate = false;
    private Container sourceContainer; // ya lo usabas en el modo container
    private int sourceIndex = -1;
}
