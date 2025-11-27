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

    [Header("Drop Targets")]
    [SerializeField] private ContainerDropTarget containerBackgroundDropTarget;

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
            btnCanvas.sortingOrder = 10;     
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
        HideTooltip();
    }

    private void Update()
    {
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
                    if (result.gameObject == consumeButtonInstance.gameObject)
                    {
                        clickedOnButton = true;
                        break;
                    }

                    if (currentSlotForConsume != null && result.gameObject == currentSlotForConsume.gameObject)
                    {
                        clickedOnActiveSlot = true;
                    }
                }

                if (!clickedOnButton && !clickedOnActiveSlot)
                {
                    HideActiveConsumeButton();
                }
            }
            else if (tooltipObject.activeSelf)
            {
                HideTooltip();
            }
        }

        if (tooltipObject != null && tooltipObject.activeSelf)
        {
            RectTransform rect = tooltipObject.GetComponent<RectTransform>();
            float currentHeight = rect.sizeDelta.y;
            float dynamicY = tooltipOffset.y + (currentHeight / 2f);
            Vector3 finalOffset = new Vector3(tooltipOffset.x, dynamicY, 0);
            tooltipObject.transform.position = Input.mousePosition + finalOffset;
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

        if (containerBackgroundDropTarget != null)
        {
            containerBackgroundDropTarget.Bind(c);
        }
        else
        {
            var dropTarget = splitRoot.GetComponentInChildren<ContainerDropTarget>();
            if (dropTarget != null) dropTarget.Bind(c);
        }

        if (pocketButtonsRoot) pocketButtonsRoot.SetActive(true);
        SetAllPockets(false);
        UpdatePocketButtonsVisuals();
    }

    public void CloseAll()
    {
        bool wasOpen = IsBackpackOpen || IsSplitOpen;

        if (backpackRoot) backpackRoot.SetActive(false);
        if (splitRoot) splitRoot.SetActive(false);
        if (pocketButtonsRoot) pocketButtonsRoot.SetActive(false);
        SetAllPockets(false);
        CurrentContainer = null;
        if (containerGridUI) containerGridUI.SetContainer(null);
        UpdatePocketButtonsVisuals();
        HideActiveConsumeButton();
        HideTooltip();
        if (containerBackgroundDropTarget != null)
            containerBackgroundDropTarget.Bind(null);

        if (wasOpen)
        {
            if (AutoSaver.Instance != null)
            {
                Debug.Log("[InventoryUI] Inventario cerrado. Guardando partida...");
                AutoSaver.Instance.TriggerAutoSave();
            }
        }
    }

    public void TogglePocket(int index)
    {
        if (!IsValidPocket(index)) return;
        bool next = !pocketPanels[index].activeSelf;
        pocketPanels[index].SetActive(next);

        if (!next)        
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

        if (!active)        
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


        if (consumeButtonInstance.transform.parent != backpackRoot.transform)
        {
            consumeButtonInstance.transform.SetParent(backpackRoot.transform, false);
        }

        RectTransform slotRect = slot.GetComponent<RectTransform>();
        RectTransform buttonRect = consumeButtonInstance.GetComponent<RectTransform>();

        Vector2 localPos;

        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas == null || canvas.renderMode == RenderMode.ScreenSpaceOverlay)
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
               backpackRoot.GetComponent<RectTransform>(),
               slotRect.position,
               null,
               out localPos);
        }
        else
        {
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                backpackRoot.GetComponent<RectTransform>(),
                slotRect.position,
                canvas.worldCamera,
                out localPos);
        }


        buttonRect.anchoredPosition = localPos + consumeButtonOffset;

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

    public static void RefreshIfAnyOpen() { }
    public void RefreshContainer(Container _) { }
    public void ForceContainerRefresh()
    {
        if (containerGridUI != null) containerGridUI.Refresh();
    }

    public void ShowTooltip(InventoryItem item)
    {
        if (tooltipObject == null || tooltipText == null || item == null) return;

        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"<b>{item.displayName}</b>");

        if (item.isConsumable && item.consumableEffects != null && item.consumableEffects.Count > 0)
        {
            sb.Append("\n<size=80%>");

            List<string> effectsText = new List<string>();

            foreach (var effect in item.consumableEffects)
            {
                string label = "";

                switch (effect.type)
                {
                    case ConsumableType.Health:
                        label = "HP";
                        break;
                    case ConsumableType.Sanity:
                        label = "SA";
                        break;
                    case ConsumableType.Stat:
                        label = effect.statType.ToString();
                        break;
                }

                string sign = (effect.amount > 0) ? "+" : "";

                effectsText.Add($"{label}{sign}{effect.amount}");
            }

            sb.Append(string.Join(", ", effectsText));

            sb.Append("</size>");
        }

        tooltipText.text = sb.ToString();

        tooltipText.ForceMeshUpdate();
        RectTransform backgroundRect = tooltipObject.GetComponent<RectTransform>();
        if (backgroundRect != null)
        {

            float padding = 5f;
            float newHeight = tooltipText.preferredHeight + padding;

            backgroundRect.sizeDelta = new Vector2(backgroundRect.sizeDelta.x, newHeight);
        }


        tooltipObject.SetActive(true);
        tooltipObject.transform.SetAsLastSibling();

        CanvasGroup canvasGroup = tooltipObject.GetComponent<CanvasGroup>();
        if (canvasGroup == null) canvasGroup = tooltipObject.AddComponent<CanvasGroup>();
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