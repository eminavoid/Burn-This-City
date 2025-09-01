using UnityEngine;

public class ModuleGridUI : MonoBehaviour
{
    public int moduleIndex;                 
    public InventoryModuleDef def;          
    public Transform gridParent;            
    public ItemSlotUI slotPrefab;

    private ItemSlotUI[] slots;
    private bool built;

    private void OnEnable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += Refresh;
        EnsureBuilt(); Refresh();
    }
    private void OnDisable()
    {
        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= Refresh;
    }

    void EnsureBuilt()
    {
        if (built || def == null || slotPrefab == null || gridParent == null) return;
        slots = new ItemSlotUI[def.Capacity];

        for (int i = 0; i < def.Capacity; i++)
        {
            var ui = Instantiate(slotPrefab, gridParent);
            slots[i] = ui;

            var drop = ui.gameObject.GetComponent<SlotDropTarget>();
            if (drop == null) drop = ui.gameObject.AddComponent<SlotDropTarget>();
            drop.moduleIndex = moduleIndex; drop.slotIndex = i;

            var drag = ui.gameObject.GetComponent<ItemSlotDrag>();
            if (drag == null) drag = ui.gameObject.AddComponent<ItemSlotDrag>();

            var bigDrop = GetComponent<ModuleGridDropTarget>();
            if (bigDrop == null) bigDrop = gameObject.AddComponent<ModuleGridDropTarget>();
            bigDrop.moduleIndex = moduleIndex;
            bigDrop.gridParent = gridParent;
            bigDrop.RebuildSlotCache();
        }
        built = true;
    }

    public void Refresh()
    {
        var man = InventoryManager.Instance; if (!built || man == null) return;
        var mod = man.GetModule(moduleIndex); if (mod == null) return;

        for (int i = 0; i < def.Capacity; i++)
        {
            var s = mod.slots[i]; var ui = slots[i];
            ui.Set(s.item, s.amount, null);

            var drag = ui.GetComponent<ItemSlotDrag>();
            drag.InitFromInventoryModule(moduleIndex, i, s.item, s.amount, ui.IconImage);
        }
    }

}
