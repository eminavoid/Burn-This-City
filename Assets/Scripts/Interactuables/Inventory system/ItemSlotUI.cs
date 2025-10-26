using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlotUI : MonoBehaviour
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
}
