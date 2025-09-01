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
    [SerializeField] private GameObject backpackRoot;  // Panel con los 4 ModuleGridUI
    [SerializeField] private GameObject splitRoot;      // opcional, si tenés cofre

    [Header("Split (derecha)")]
    [SerializeField] private ContainerGridUI containerGridUI;

    public Container CurrentContainer { get; private set; }

    public bool IsBackpackOpen => backpackRoot && backpackRoot.activeSelf;
    public bool IsSplitOpen => splitRoot && splitRoot.activeSelf;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        if (backpackRoot) backpackRoot.SetActive(false);
        if (splitRoot) splitRoot.SetActive(false);
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

    public void OpenBackpack()
    {
        if (splitRoot) splitRoot.SetActive(false);
        if (backpackRoot) backpackRoot.SetActive(true); // Los ModuleGridUI refrescan en OnEnable
    }

    public void CloseAll()
    {
        if (splitRoot) splitRoot.SetActive(false);
        if (backpackRoot) backpackRoot.SetActive(false);
        CurrentContainer = null;
        if (containerGridUI != null) containerGridUI.SetContainer(null);
    }

    // Helper que podés llamar desde Container si querés refrescar algo externo
    public static void RefreshIfAnyOpen()
    {
        // Los ModuleGridUI se repintan con OnInventoryChanged; no hace falta nada acá.
    }

    // Opcional para split (cuando implementes cofre de nuevo)
    public void OpenSplit(Container c)
    {
        CurrentContainer = c;

        if (backpackRoot) backpackRoot.SetActive(true);
        if (splitRoot) splitRoot.SetActive(true);

        if (containerGridUI != null) containerGridUI.SetContainer(c);
    }
}

