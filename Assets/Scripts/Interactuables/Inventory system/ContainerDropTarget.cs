using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class ContainerDropTarget : MonoBehaviour, IDropHandler
{
    [HideInInspector] public Container boundContainer;

    public void Bind(Container c) { boundContainer = c; }

    public void OnDrop(PointerEventData e)
    {
        Debug.Log($"[Drop] ContainerDropTarget OnDrop over {name}, bound={(boundContainer ? boundContainer.name : "null")}");
        ModuleGridDropTarget.LastDropScreenPos = e.position;
        DragAndDropController.Instance?.NotifyDropped(gameObject);
    }
}

