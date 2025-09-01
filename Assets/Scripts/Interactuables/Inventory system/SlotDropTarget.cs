using UnityEngine;
using UnityEngine.EventSystems;

public class SlotDropTarget : MonoBehaviour, IDropHandler
{
    public int moduleIndex;
    public int slotIndex;
    public void OnDrop(PointerEventData eventData)
    {
        DragAndDropController.Instance?.NotifyDropped(gameObject);
    }
}
