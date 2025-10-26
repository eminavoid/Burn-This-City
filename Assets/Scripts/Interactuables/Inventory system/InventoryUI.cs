using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class InventoryUI : MonoBehaviour
{
    public static InventoryUI Instance { get; private set; }

    [Header("Input")]
    [SerializeField] private InputActionReference toggleInventory;

    [Header("Roots")]
    [SerializeField] private GameObject backpackRoot;
    [SerializeField] private GameObject splitRoot;

    [Header("Pocket Buttons (selector estético)")]
    [SerializeField] private GameObject pocketButtonsRoot;
    [SerializeField] private UnityEngine.UI.Button[] pocketButtons = new UnityEngine.UI.Button[4];

    [Header("Pocket Panels (uno por módulo)")]
    [SerializeField] private GameObject[] pocketPanels = new GameObject[4];

    [Header("Visual de botón deshabilitado")]
    [SerializeField, Range(0f, 1f)] private float disabledAlpha = 0.5f;

    [Header("Consume Button")]
    [SerializeField] private Button consumeButtonPrefab;
    [SerializeField] private Vector2 consumeButtonOffset;
    private Button consumeButtonInstance;
    private ItemSlotUI currentSlotForConsume;

    [Header("Split (derecha/cofre)")]
    [SerializeField] private ContainerGridUI containerGridUI;
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
        SetAllPockets(false);
        if (pocketButtonsRoot) pocketButtonsRoot.SetActive(false);

        for (int i = 0; i < pocketButtons.Length; i++)
        {
            int idx = i;
            if (pocketButtons[i] != null)
                pocketButtons[i].onClick.AddListener(() => TogglePocket(idx));
        }
        UpdatePocketButtonsVisuals();
        if (consumeButtonPrefab != null && backpackRoot != null)
        {
            consumeButtonInstance = Instantiate(consumeButtonPrefab, backpackRoot.transform);

            Canvas btnCanvas = consumeButtonInstance.gameObject.AddComponent<Canvas>();
            btnCanvas.overrideSorting = true;
            btnCanvas.sortingOrder = 10; // Dibujar encima de todo
            consumeButtonInstance.gameObject.AddComponent<GraphicRaycaster>();

            consumeButtonInstance.gameObject.SetActive(false);
        }
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
        HideActiveConsumeButton();
    }

    private void Update()
    {
        // Detecta clic fuera del botón
        if (Input.GetMouseButtonDown(0))
        {
            // Verificamos si el clic fue sobre el botón mismo
            if (consumeButtonInstance != null && consumeButtonInstance.gameObject.activeSelf)
            {
                if (!EventSystem.current.IsPointerOverGameObject(Input.GetTouch(0).fingerId))
                {
                    // Si no hay un objeto de UI bajo el clic, o si lo hay pero NO es el botón
                    PointerEventData eventData = new PointerEventData(EventSystem.current);
                    eventData.position = Input.mousePosition;
                    List<RaycastResult> results = new List<RaycastResult>();
                    EventSystem.current.RaycastAll(eventData, results);

                    bool clickedOnButton = false;
                    foreach (var result in results)
                    {
                        if (result.gameObject == consumeButtonInstance.gameObject)
                        {
                            clickedOnButton = true;
                            break;
                        }
                    }

                    if (!clickedOnButton)
                        HideActiveConsumeButton();
                }
            }
            // Lógica original para clic fuera de cualquier UI
            else if (!EventSystem.current.IsPointerOverGameObject())
            {
                HideActiveConsumeButton();
            }
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

        if (containerGridUI != null)
        {
            containerGridUI.SetContainer(c);
            containerGridUI.Refresh();
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
        HideActiveConsumeButton();
    }

    // --------- Pockets (abrir/cerrar individual) ---------
    public void TogglePocket(int index)
    {
        if (!IsValidPocket(index)) return;
        bool next = !pocketPanels[index].activeSelf;
        pocketPanels[index].SetActive(next);

        if (!next) // Si se acaba de CERRAR el pocket
        {
            HideActiveConsumeButton();
        }

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
        HideActiveConsumeButton();
    }

    private void SetAllPockets(bool active)
    {
        if (pocketPanels == null) return;
        for (int i = 0; i < pocketPanels.Length; i++)
            if (pocketPanels[i] != null)
                pocketPanels[i].SetActive(active);

        if (!active) // Si se están CERRANDO todos los pockets
        {
            HideActiveConsumeButton();
        }
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
            btn.interactable = !isOpen;
        }
    }
    public void ShowConsumeButtonFor(ItemSlotUI slot)
    {
        if (currentSlotForConsume == slot && consumeButtonInstance.gameObject.activeSelf)
        {
            HideActiveConsumeButton();
            return;
        }

        if (slot.CurrentItem == null || !slot.CurrentItem.isConsumable)
        {
            HideActiveConsumeButton(); // Oculta si había uno visible
            return;
        }

        currentSlotForConsume = slot;
        consumeButtonInstance.gameObject.SetActive(true);

        consumeButtonInstance.transform.SetParent(slot.transform);
        consumeButtonInstance.GetComponent<RectTransform>().anchoredPosition = consumeButtonOffset;
        consumeButtonInstance.transform.SetParent(backpackRoot.transform);

        consumeButtonInstance.onClick.RemoveAllListeners();
        consumeButtonInstance.onClick.AddListener(slot.OnConsumeClicked);
    }

    public void HideActiveConsumeButton()
    {
        if (consumeButtonInstance != null)
        {
            consumeButtonInstance.gameObject.SetActive(false);
            consumeButtonInstance.onClick.RemoveAllListeners();
        }
        currentSlotForConsume = null;
    }

    // Helpers de compatibilidad
    public static void RefreshIfAnyOpen() { }
    public void RefreshContainer(Container _) { }
    public void ForceContainerRefresh()
    {
        if (containerGridUI != null) containerGridUI.Refresh();
    }
}