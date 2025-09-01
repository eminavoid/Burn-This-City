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

        // Mostrar icono si hay sprite
        if (item != null && item.icon != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = true;      // <- mantenerlo habilitado
        }
        else
        {
            iconImage.sprite = null;
            iconImage.enabled = false;     // <- sólo si REALMENTE no hay icono
        }

        countText.text = (amount > 1) ? amount.ToString() : "";
    }

    public void Clear()
    {
        iconImage.enabled = false;
        iconImage.sprite = null;
        countText.text = "";
        onClick = null;
    }

    public void OnClick() => onClick?.Invoke();
    public Image IconImage => iconImage;

}
