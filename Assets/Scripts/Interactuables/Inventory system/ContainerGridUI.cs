using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ContainerGridUI : MonoBehaviour
{
    [Header("Grid")]
    public Transform gridParent;
    public ItemSlotUI slotPrefab;

    private Container current;
    public Container Current => current;

    private void OnEnable()
    {
        if (current != null) { current.OnChanged -= Refresh; current.OnChanged += Refresh; }
        Refresh();
    }

    private void OnDisable()
    {
        if (current != null) current.OnChanged -= Refresh;
    }

    public void SetContainer(Container c)
    {
        if (current == c) { Refresh(); return; }
        if (current != null) current.OnChanged -= Refresh;
        current = c;
        if (current != null) current.OnChanged += Refresh;
        Debug.Log($"[ContainerGridUI] SetContainer -> {(current ? current.name : "null")}");
        Refresh();
    }

    public void Refresh()
    {
        if (gridParent == null) { Debug.LogWarning("[ContainerGridUI] gridParent null"); return; }

        for (int i = gridParent.childCount - 1; i >= 0; i--) Destroy(gridParent.GetChild(i).gameObject);

        if (current == null || current.contents == null || current.contents.Count == 0)
        {
            Debug.Log("[ContainerGridUI] vacío");
            ForceLayout();
            return;
        }

        Debug.Log($"[ContainerGridUI] Pintando stacks={current.contents.Count}");

        for (int i = 0; i < current.contents.Count; i++)
        {
            var ia = current.contents[i];
            if (ia == null || ia.item == null || ia.amount <= 0) continue;

            var slot = Instantiate(slotPrefab, gridParent);
            int capturedIndex = i;

            slot.Set(ia.item, ia.amount, onClick: () =>
            {
                bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                if (ctrl && current.contents[capturedIndex].amount > 1) current.LootPartial(capturedIndex, 1);
                else current.LootOne(capturedIndex);
            });

            var drag = slot.GetComponent<ItemSlotDrag>() ?? slot.gameObject.AddComponent<ItemSlotDrag>();
            drag.InitFromContainer(current, capturedIndex, ia.item, ia.amount, slot.IconImage);
        }

        ForceLayout();
    }

    private void ForceLayout()
    {
        Canvas.ForceUpdateCanvases();
        var rt = gridParent as RectTransform;
        if (rt) UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(rt);
        Canvas.ForceUpdateCanvases();
    }
}
