using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using UnityEngine.EventSystems;

public class ItemSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    private InventoryItem currentItem;
    private int currentAmount;
    private Action onClick;

    public InventoryItem CurrentItem => currentItem;

    public void Set(InventoryItem item, int amount, Action onClick)
    {
        this.currentItem = item;
        this.currentAmount = amount;
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
            // NEW: Esconde el Tooltip inmediatamente al hacer click
            InventoryUI.Instance.HideTooltip();

            // Llama a la lógica que muestra/oculta el botón de consumir
            InventoryUI.Instance.ShowConsumeButtonFor(this);
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
        InventoryManager.Instance.TryConsume(currentItem,1);
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
            // Solo muestra el Tooltip si el Botón Consumir NO está visible para este slot
            if (InventoryUI.Instance.GetCurrentSlotForConsume() != this)
            {
                InventoryUI.Instance.ShowTooltip(currentItem.displayName);
            }
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        // El Tooltip debe ocultarse siempre que el puntero salga,
        // a menos que el botón de consumir esté activo (pero la lógica del Tooltip ya lo oculta)
        if (InventoryUI.Instance != null)
        {
            // Esto asegura que el Tooltip desaparezca cuando el mouse se mueve hacia el botón consumir
            InventoryUI.Instance.HideTooltip();
        }
    }
}
