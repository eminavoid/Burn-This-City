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

    [Header("Tooltip Settings")]
    [SerializeField] private GameObject tooltipObject;
    [SerializeField] private TMP_Text tooltipText;
    [SerializeField] private Vector3 tooltipOffset = new Vector3(15, -15, 0);

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
        // --- Lógica del Botón Consumir ---

        // Detecta clic fuera del botón o fuera de un slot (asumiendo que Click() en el slot llama ShowConsumeButtonFor)
        if (Input.GetMouseButtonDown(0))
        {
            if (consumeButtonInstance != null && consumeButtonInstance.gameObject.activeSelf)
            {
                PointerEventData eventData = new PointerEventData(EventSystem.current)
                {
                    position = Input.mousePosition
                };
                List<RaycastResult> results = new List<RaycastResult>();
                EventSystem.current.RaycastAll(eventData, results);

                bool clickedOnButton = false;
                bool clickedOnActiveSlot = false;

                foreach (var result in results)
                {
                    // Detectar si se hizo clic en el botón
                    if (result.gameObject == consumeButtonInstance.gameObject)
                    {
                        clickedOnButton = true;
                        break;
                    }

                    // Detectar si se hizo clic en el slot que tiene el botón activo
                    if (currentSlotForConsume != null && result.gameObject == currentSlotForConsume.gameObject)
                    {
                        clickedOnActiveSlot = true;
                        // NO romper el loop, aún necesitamos revisar si el botón está encima.
                    }
                }

                // Ocultar si el clic no fue ni en el botón ni en el slot (o si se hizo clic en otro slot/UI)
                // Si haces clic en otro lado (o en el mismo slot por segunda vez), oculta el botón.
                if (!clickedOnButton && !clickedOnActiveSlot)
                {
                    HideActiveConsumeButton();
                }
            }
            // Asegura que, si no hay botón activo, el Tooltip tampoco lo esté.
            else if (tooltipObject.activeSelf)
            {
                // Esto es una medida de seguridad, HideTooltip() ya se debería llamar en OnPointerExit
                HideTooltip();
            }
        }

        // --- Lógica del Tooltip (Seguir al Mouse) ---

        if (tooltipObject != null && tooltipObject.activeSelf)
        {
            tooltipObject.transform.position = Input.mousePosition + tooltipOffset;
        }
    }

    private void OnToggle(InputAction.CallbackContext _)
    {
        if (IsSplitOpen || IsBackpackOpen) CloseAll();
        else OpenBackpack();
    }
    
    public void ToggleInventory()
    {
        if (IsBackpackOpen || IsSplitOpen)
            CloseAll();
        else
            OpenBackpack();
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
            HideActiveConsumeButton();
            return;
        }

        currentSlotForConsume = slot;
        consumeButtonInstance.gameObject.SetActive(true);

        // --- LÓGICA CORREGIDA PARA SCREEN SPACE - OVERLAY ---

        // 1. Aseguramos que el botón permanezca como hijo de backpackRoot para evitar la interrupción de eventos
        if (consumeButtonInstance.transform.parent != backpackRoot.transform)
        {
            // Usamos 'false' para no preservar la posición en el mundo, sino solo la local
            consumeButtonInstance.transform.SetParent(backpackRoot.transform, false);
        }

        RectTransform slotRect = slot.GetComponent<RectTransform>();
        RectTransform buttonRect = consumeButtonInstance.GetComponent<RectTransform>();

        // 2. Calculamos la posición local del slot dentro del backpackRoot.
        // Convertimos la posición del slot (que es un punto en el canvas/pantalla)
        // a coordenadas locales dentro del RectTransform del backpackRoot.
        Vector2 localPos;

        // Intentamos obtener el canvas. Si es overlay, pasamos 'null' como cámara/canvas.
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            // Para Overlay, usamos null en la cámara/canvas.
            // La posición del slot ya está en coordenadas de pantalla.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
               backpackRoot.GetComponent<RectTransform>(),
               slotRect.position,
               null,
               out localPos);
        }
        else
        {
            // Si por alguna razón no es overlay (aunque dijiste que sí), volvemos al método con cámara.
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backpackRoot.GetComponent<RectTransform>(),
                slotRect.position,
                canvas.worldCamera,
                out localPos);
        }


        // 3. Aplicamos la posición local más el offset
        buttonRect.anchoredPosition = localPos + consumeButtonOffset;

        // --- FIN DE LÓGICA CORREGIDA ---

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

    public void ShowTooltip(string itemName)
    {
        if (tooltipObject == null || tooltipText == null) return;

        tooltipText.text = itemName;
        tooltipObject.SetActive(true);
        tooltipObject.transform.SetAsLastSibling();

        // NEW FIX: Ensure the tooltip does not block mouse clicks on other UI elements.
        CanvasGroup canvasGroup = tooltipObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = tooltipObject.AddComponent<CanvasGroup>();
        }

        // This is the key line: tells the system to ignore the tooltip for raycasts.
        canvasGroup.blocksRaycasts = false;
    }
    public void HideTooltip()
    {
        if (tooltipObject != null)
            tooltipObject.SetActive(false);
    }
    public ItemSlotUI GetCurrentSlotForConsume()
    {
        return currentSlotForConsume;
    }
}