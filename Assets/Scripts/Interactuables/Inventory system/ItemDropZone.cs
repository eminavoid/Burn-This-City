using UnityEngine;
using UnityEngine.EventSystems;

public class ItemDropZone : MonoBehaviour, IDropHandler
{
    public enum DropKind { Backpack, Container }
    public DropKind kind = DropKind.Backpack; // set from Inspector

    // Marker: actual logic is handled in the Drag controller’s callback.
    public void OnDrop(PointerEventData eventData)
    {
        if (DragAndDropController.Instance != null)
            DragAndDropController.Instance.NotifyDropped(gameObject);
    }
}
