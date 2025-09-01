using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ItemSlotDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public enum SlotSource { Inventory, Container }

    [SerializeField] private Image iconImage; // asignalo desde ItemSlotUI (ver abajo)
    private SlotSource source;
    private InventoryItem item;
    private int amount;

    private Container sourceContainer;
    private int sourceIndex = -1;

    // ---- Inicializadores (los llama InventoryUI al crear los slots) ----
    public void InitFromInventory(InventoryItem item, int amount, Image icon)
    {
        source = SlotSource.Inventory;
        this.item = item;
        this.amount = amount;
        this.iconImage = icon;
        sourceContainer = null;
        sourceIndex = -1;
    }

    public void InitFromContainer(Container container, int index, InventoryItem item, int amount, Image icon)
    {
        source = SlotSource.Container;
        sourceContainer = container;
        sourceIndex = index;
        this.item = item;
        this.amount = amount;
        this.iconImage = icon;
    }
    // -------------------------------------------------------------------

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (item == null || amount <= 0) return;

        var cg = GetComponent<CanvasGroup>();
        if (cg == null) cg = gameObject.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.alpha = 0.6f;

        Sprite icon = (iconImage != null && iconImage.sprite != null) ? iconImage.sprite : (item != null ? item.icon : null);

        DragAndDropController.Instance?.Begin(icon, dropTarget =>
        {
            var dz = dropTarget != null ? dropTarget.GetComponent<ItemDropZone>() : null;
            if (dz == null) return;

            bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);

            if (source == SlotSource.Container && dz.kind == ItemDropZone.DropKind.Backpack)
            {
                if (sourceContainer != null && sourceIndex >= 0)
                {
                    int currentStack =
                        (sourceContainer.contents != null &&
                         sourceIndex >= 0 &&
                         sourceIndex < sourceContainer.contents.Count)
                            ? sourceContainer.contents[sourceIndex].amount
                            : amount;

                    int moveAmount = ctrl ? 1 : currentStack;

                    if (moveAmount < currentStack)
                        sourceContainer.LootPartial(sourceIndex, moveAmount); 
                    else
                        sourceContainer.LootOne(sourceIndex);
                }
            }
            // ——— Backpack -> Container ———
            else if (source == SlotSource.Inventory && dz.kind == ItemDropZone.DropKind.Container)
            {
                var ui = InventoryUI.Instance;
                var targetContainer = ui != null ? ui.CurrentContainer : null;
                if (targetContainer != null)
                {
                    int moveAmount = ctrl ? 1 : amount;
                    targetContainer.TryReceiveFromInventory(item, moveAmount);
                }
            }
            // Mismo lado (inventory->inventory / container->container) se ignora por ahora.
        });
    }

    public void OnDrag(PointerEventData eventData)
    {
        DragAndDropController.Instance?.Move(eventData.position);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        var cg = GetComponent<CanvasGroup>();
        if (cg != null)
        {
            cg.blocksRaycasts = true;
            cg.alpha = 1f;
        }
        DragAndDropController.Instance?.Cancel();
    }
}
