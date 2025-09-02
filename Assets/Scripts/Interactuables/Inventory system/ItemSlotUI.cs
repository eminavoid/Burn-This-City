using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    private Action onClick;

    public void Set(InventoryItem item, int amount, Action onClick)
    {
        this.onClick = onClick;
        if (item != null && item.icon != null) { iconImage.enabled = true; iconImage.sprite = item.icon; }
        else { iconImage.enabled = false; iconImage.sprite = null; }
        countText.text = amount > 1 ? amount.ToString() : "";
    }

    public void Click() => onClick?.Invoke();
    public Image IconImage => iconImage;
}
