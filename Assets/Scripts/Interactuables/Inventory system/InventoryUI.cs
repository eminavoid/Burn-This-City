using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference toggleInventory; // tecla I

    [Header("ROOTS")]
    [SerializeField] private GameObject backpackRoot;   // SOLO mochila
    [SerializeField] private GameObject splitRoot;      // mochila + cofre

    [Header("Backpack (SOLO)")]
    [SerializeField] private Transform backpackSoloGridParent;  // GridLayoutGroup SOLO
    [SerializeField] private ItemSlotUI backpackSoloSlotPrefab;
    [SerializeField] private Button backpackSoloCloseButton;

    [Header("Backpack (SPLIT, izquierda)")]
    [SerializeField] private Transform backpackSplitGridParent; // GridLayoutGroup SPLIT
    [SerializeField] private ItemSlotUI backpackSplitSlotPrefab;

    [Header("Container (SPLIT, derecha)")]
    [SerializeField] private Transform containerGridParent; // GridLayoutGroup SPLIT
    [SerializeField] private ItemSlotUI containerSlotPrefab;
    [SerializeField] private Button splitLootAllButton;
    [SerializeField] private Button splitCloseButton;

    private readonly List<ItemSlotUI> soloSlots = new();
    private readonly List<ItemSlotUI> splitBackpackSlots = new();
    private readonly List<ItemSlotUI> splitContainerSlots = new();

    private Container currentContainer;

    public bool IsBackpackOpen => backpackRoot != null && backpackRoot.activeSelf;
    public bool IsSplitOpen => splitRoot != null && splitRoot.activeSelf;

    public static void RefreshIfAnyOpen()
    {
        if (Instance == null) return;
        if (Instance.IsBackpackOpen || Instance.IsSplitOpen)
            Instance.ForceRefresh();
    }

    public void ForceRefresh()
    {
        if (IsBackpackOpen) RefreshBackpackSolo();
        if (IsSplitOpen)
        {
            RefreshBackpackSplit();
            RefreshContainer(currentContainer);
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        HideAll();
    }

    private void OnEnable()
    {
        if (toggleInventory != null)
        {
            toggleInventory.action.Enable();
            toggleInventory.action.started += OnToggleBackpack;
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged += OnInventoryChanged;
    }

    private void OnDisable()
    {
        if (toggleInventory != null)
        {
            toggleInventory.action.started -= OnToggleBackpack;
            toggleInventory.action.Disable();
        }

        if (InventoryManager.Instance != null)
            InventoryManager.Instance.OnInventoryChanged -= OnInventoryChanged;
    }

    private void OnInventoryChanged() => RefreshIfAnyOpen();

    private void OnToggleBackpack(InputAction.CallbackContext _)
    {
        // Si está en modo split (cofre abierto), ignorá la tecla I para esta demo
        if (IsSplitOpen) return;

        if (IsBackpackOpen) CloseAll();
        else OpenBackpack();
    }

    // -------- API PÚBLICA --------

    /// <summary> Abre SOLO la mochila (tecla I). </summary>
    public void OpenBackpack()
    {
        currentContainer = null;
        splitRoot.SetActive(false);

        WireBackpackSoloButtons();
        backpackRoot.SetActive(true);
        RefreshBackpackSolo();
    }

    /// <summary> Abre SPLIT: mochila + cofre. </summary>
    public void OpenSplit(Container container)
    {
        currentContainer = container;

        backpackRoot.SetActive(false);
        WireSplitButtons();

        splitRoot.SetActive(true);
        RefreshBackpackSplit();
        RefreshContainer(container);
    }

    public void CloseAll() => HideAll();

    public void RefreshContainer(Container container)
    {
        // Limpiar grid derecha
        foreach (var s in splitContainerSlots) if (s) Destroy(s.gameObject);
        splitContainerSlots.Clear();

        if (!IsSplitOpen || container == null || container.contents == null) return;

        Debug.Log($"[InventoryUI] RefreshContainer: splitOpen={IsSplitOpen} containerNull={container == null} contentsCount={(container?.contents?.Count ?? -1)}");

        for (int i = 0; i < container.contents.Count; i++)
        {
            var ia = container.contents[i];
            var slot = Instantiate(containerSlotPrefab, containerGridParent);
            int capturedIndex = i;

            slot.Set(ia.item, ia.amount, () =>
            {
                // Click en tile del cofre => loot de ese stack
                container.LootOne(capturedIndex);
                // Al lootear, se refresca container y mochila (en Container.LootOne ya lo hacemos, pero redundar es barato)
                RefreshBackpackSplit();
                RefreshContainer(container);
            });

            splitContainerSlots.Add(slot);
        }
    }

    // -------- Internos --------

    private void RefreshBackpackSolo()
    {
        foreach (var s in soloSlots) if (s) Destroy(s.gameObject);
        soloSlots.Clear();

        var entries = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetAllEntries()
            : new List<InventoryEntry>();

        foreach (var e in entries)
        {
            var slot = Instantiate(backpackSoloSlotPrefab, backpackSoloGridParent);
            slot.Set(e.item, e.amount, () =>
            {
                Debug.Log($"Usar/Click SOLO: {e.item.displayName}");
            });
            soloSlots.Add(slot);
        }
    }

    private void RefreshBackpackSplit()
    {
        foreach (var s in splitBackpackSlots) if (s) Destroy(s.gameObject);
        splitBackpackSlots.Clear();

        var entries = InventoryManager.Instance != null
            ? InventoryManager.Instance.GetAllEntries()
            : new List<InventoryEntry>();

        foreach (var e in entries)
        {
            var slot = Instantiate(backpackSplitSlotPrefab, backpackSplitGridParent);
            slot.Set(e.item, e.amount, () =>
            {
                Debug.Log($"Usar/Click SPLIT: {e.item.displayName}");
            });
            splitBackpackSlots.Add(slot);
        }
    }

    private void WireBackpackSoloButtons()
    {
        if (backpackSoloCloseButton != null)
        {
            backpackSoloCloseButton.onClick.RemoveAllListeners();
            backpackSoloCloseButton.onClick.AddListener(CloseAll);
        }
    }

    private void WireSplitButtons()
    {
        if (splitLootAllButton != null)
        {
            splitLootAllButton.onClick.RemoveAllListeners();
            splitLootAllButton.onClick.AddListener(() =>
            {
                currentContainer?.LootAll();
                RefreshBackpackSplit();
                RefreshContainer(currentContainer);
            });
        }

        if (splitCloseButton != null)
        {
            splitCloseButton.onClick.RemoveAllListeners();
            splitCloseButton.onClick.AddListener(CloseAll);
        }
    }

    private void HideAll()
    {
        backpackRoot.SetActive(false);
        splitRoot.SetActive(false);
        currentContainer = null;
    }

}
