using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference toggleInventory;

    [Header("Roots")]
    [SerializeField] private GameObject backpackRoot;  // panel padre de la mochila
    [SerializeField] private GameObject splitRoot;      // panel del cofre (si lo usás)

    [Header("Pocket Buttons (selector estético)")]
    [SerializeField] private GameObject pocketButtonsRoot;   // contenedor de los 4 botones
    [SerializeField] private UnityEngine.UI.Button[] pocketButtons = new UnityEngine.UI.Button[4];

    [Header("Pocket Panels (uno por módulo)")]
    // Cada pocket es el GameObject que contiene el ModuleGridUI correspondiente
    [SerializeField] private GameObject[] pocketPanels = new GameObject[4];

    [Header("Visual de botón deshabilitado")]
    [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.5f;

    [Header("Split (derecha/cofre)")]
    [SerializeField] private ContainerGridUI containerGridUI; // opcional, si usás cofre
    public ContainerGridUI ContainerGridUI => containerGridUI;

    public bool IsBackpackOpen => backpackRoot && backpackRoot.activeSelf;
    public bool IsSplitOpen => splitRoot && splitRoot.activeSelf;
    public Container CurrentContainer { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (backpackRoot) backpackRoot.SetActive(false);
        if (splitRoot) splitRoot.SetActive(false);

        // Por estética: ocultar los pockets y mostrar sólo los botones al abrir
        SetAllPockets(false);
        if (pocketButtonsRoot) pocketButtonsRoot.SetActive(false);

        // Wire automático de botones si están asignados
        for (int i = 0; i < pocketButtons.Length; i++)
        {
            int idx = i;
            if (pocketButtons[i] != null)
                pocketButtons[i].onClick.AddListener(() => TogglePocket(idx));
        }
        UpdatePocketButtonsVisuals();
    }

    private void OnEnable()
    {
        if (toggleInventory != null)
        {
            toggleInventory.action.Enable();
            toggleInventory.action.started += OnToggle;
        }
    }

    private void OnDisable()
    {
        if (toggleInventory != null)
        {
            toggleInventory.action.started -= OnToggle;
            toggleInventory.action.Disable();
        }
    }

    private void OnToggle(InputAction.CallbackContext _)
    {
        if (IsSplitOpen || IsBackpackOpen) CloseAll();
        else OpenBackpack();
    }

    // --------- Abrir/Cerrar modos ---------
    public void OpenBackpack()
    {
        CurrentContainer = null;
        if (splitRoot) splitRoot.SetActive(false);
        if (backpackRoot) backpackRoot.SetActive(true);

        if (pocketButtonsRoot) pocketButtonsRoot.SetActive(true);
        SetAllPockets(false);
        UpdatePocketButtonsVisuals();
    }

    public void OpenSplit(Container c)
    {
        CurrentContainer = c;

        if (backpackRoot) backpackRoot.SetActive(true);
        if (splitRoot) splitRoot.SetActive(true);

        if (containerGridUI == null && splitRoot != null)
            containerGridUI = splitRoot.GetComponentInChildren<ContainerGridUI>(true);

        Debug.Log($"[InventoryUI] OpenSplit: container={(c ? c.name : "null")} gridUI={(containerGridUI ? containerGridUI.name : "null")}");

        if (containerGridUI != null) { containerGridUI.SetContainer(c); containerGridUI.Refresh(); }

        var gridGo = containerGridUI ? containerGridUI.gridParent : null;
        if (gridGo != null)
        {
            var contDrop = gridGo.GetComponent<ContainerDropTarget>() ?? gridGo.gameObject.AddComponent<ContainerDropTarget>();
            contDrop.Bind(c);
        }

        if (pocketButtonsRoot) pocketButtonsRoot.SetActive(true);
        SetAllPockets(false);
        UpdatePocketButtonsVisuals();
    }




    public void CloseAll()
    {
        if (backpackRoot) backpackRoot.SetActive(false);
        if (splitRoot) splitRoot.SetActive(false);
        if (pocketButtonsRoot) pocketButtonsRoot.SetActive(false);
        SetAllPockets(false);
        CurrentContainer = null;
        if (containerGridUI) containerGridUI.SetContainer(null);
        UpdatePocketButtonsVisuals();
    }

    // --------- Pockets (abrir/cerrar individual) ---------
    public void TogglePocket(int index)
    {
        if (!IsValidPocket(index)) return;
        bool next = !pocketPanels[index].activeSelf;
        pocketPanels[index].SetActive(next);
        UpdatePocketButtonsVisuals();
    }

    public void OpenPocket(int index)
    {
        if (!IsValidPocket(index)) return;
        pocketPanels[index].SetActive(true);
        UpdatePocketButtonsVisuals();
    }

    public void ClosePocket(int index)
    {
        if (!IsValidPocket(index)) return;
        pocketPanels[index].SetActive(false);
        UpdatePocketButtonsVisuals();
    }

    private void SetAllPockets(bool active)
    {
        if (pocketPanels == null) return;
        for (int i = 0; i < pocketPanels.Length; i++)
            if (pocketPanels[i] != null)
                pocketPanels[i].SetActive(active);
    }

    private bool IsValidPocket(int index)
    {
        return pocketPanels != null && index >= 0 && index < pocketPanels.Length && pocketPanels[index] != null;
    }
    private void UpdatePocketButtonsVisuals()
    {
        if (pocketButtons == null || pocketPanels == null) return;

        for (int i = 0; i < pocketButtons.Length; i++)
        {
            var btn = pocketButtons[i];
            if (btn == null) continue;

            bool isOpen = (i < pocketPanels.Length && pocketPanels[i] != null && pocketPanels[i].activeSelf);

            // deshabilitar click cuando está abierto
            btn.interactable = !isOpen;
        }
    }
    // Helpers de compatibilidad
    public static void RefreshIfAnyOpen() { /* no-op, grids se repintan solos */ }
    public void RefreshContainer(Container _) { /* no-op */ }

    public void ForceContainerRefresh()
    {
        if (containerGridUI != null) containerGridUI.Refresh();
    }
}

