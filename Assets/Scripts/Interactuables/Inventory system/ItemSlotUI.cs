using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;

    private int moduleIndex;
    private int slotIndex;

    private InventoryItem currentItem;
    private int currentAmount;
    private Action onClick;

    public InventoryItem CurrentItem => currentItem;

    public void Set(InventoryItem item, int amount, int modIdx, int slotIdx, Action onClick)
    {
        this.currentItem = item;
        this.currentAmount = amount;

        this.moduleIndex = modIdx;
        this.slotIndex = slotIdx;

        this.onClick = onClick;

        if (item != null && item.icon != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = item.icon;
        }
        else
        {
            iconImage.enabled = false;
            iconImage.sprite = null;
        }

        countText.text = amount > 1 ? amount.ToString() : "";
    }

    public void Click()
    {
        onClick?.Invoke();

        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.HideTooltip();
            if (moduleIndex >= 0)
            {
                InventoryUI.Instance.ShowConsumeButtonFor(this);
            }
        }
    }


    public void OnConsumeClicked()
    {
        if (currentItem == null || !currentItem.isConsumable)
            return;

        foreach (var effect in currentItem.consumableEffects)
        {
            switch (effect.type)
            {
                case ConsumableType.Health:
                    SurvivabilityManager.Instance.ModifyHealth(effect.amount);
                    break;
                case ConsumableType.Sanity:
                    SurvivabilityManager.Instance.ModifySanity(effect.amount);
                    break;
                case ConsumableType.Stat:
                    StatManager.Instance.IncrementStat(effect.statType, (int)effect.amount);
                    break;
            }
        }

        // Remover el ítem del inventario
        InventoryManager.Instance.ConsumeFromSlot(moduleIndex, slotIndex, 1);
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.HideActiveConsumeButton();
        }
    }

    public Image IconImage => iconImage;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (currentItem != null && InventoryUI.Instance != null)
        {
            if (InventoryUI.Instance.GetCurrentSlotForConsume() != this)
            {
                InventoryUI.Instance.ShowTooltip(currentItem);
            }
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        if (InventoryUI.Instance != null)
        {
            InventoryUI.Instance.HideTooltip();
        }
    }
}
