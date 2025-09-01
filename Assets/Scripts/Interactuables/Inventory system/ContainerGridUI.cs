using System.Collections.Generic;
using UnityEngine;

public class ContainerGridUI : MonoBehaviour
{
    [Header("Grid")]
    public Transform gridParent;      // Rect con GridLayoutGroup
    public ItemSlotUI slotPrefab;

    private readonly List<ItemSlotUI> slots = new();
    private Container current;

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

        Rebuild();
        Refresh();
    }

    private void Rebuild()
    {
        // limpiar
        foreach (Transform ch in gridParent) Destroy(ch.gameObject);
        slots.Clear();
    }

    public void Refresh()
    {
        if (gridParent == null) return;

        // reconstruimos simple en cada refresh (contenedor suele tener pocos items)
        Rebuild();

        if (current == null || current.contents == null) return;

        for (int i = 0; i < current.contents.Count; i++)
        {
            var ia = current.contents[i];
            var slot = Instantiate(slotPrefab, gridParent);
            slots.Add(slot);

            int capturedIndex = i;

            // Visual
            slot.Set(ia.item, ia.amount, onClick: () =>
            {
                // Click = loot 1 si CTRL, si no loot all del stack
                bool ctrl = Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl);
                if (ctrl && ia.amount > 1) current.LootPartial(capturedIndex, 1);
                else current.LootOne(capturedIndex);
            });

            // Drag desde contenedor  mochila
            var drag = slot.GetComponent<ItemSlotDrag>();
            if (drag == null) drag = slot.gameObject.AddComponent<ItemSlotDrag>();
            drag.InitFromContainer(current, capturedIndex, ia.item, ia.amount, slot.IconImage);
        }
    }
}
