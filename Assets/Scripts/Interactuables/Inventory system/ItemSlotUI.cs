using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class ItemSlotUI : MonoBehaviour
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text countText;
    [SerializeField] private Button consumeButtonPrefab; // nuevo prefab asignable desde el inspector

    private Button consumeButtonInstance;
    private InventoryItem currentItem;
    private int currentAmount;
    private Action onClick;

    private void Start()
    {
        if (consumeButtonPrefab != null)
        {
            consumeButtonInstance = Instantiate(consumeButtonPrefab, transform.parent);
            consumeButtonInstance.gameObject.SetActive(false);
            consumeButtonInstance.onClick.AddListener(OnConsumeClicked);
        }
    }

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

        if (consumeButtonInstance != null)
            consumeButtonInstance.gameObject.SetActive(false);
    }

    public void Click()
    {
        onClick?.Invoke();
        TryShowConsumeButton();
    }

    private void TryShowConsumeButton()
    {
        if (currentItem == null || consumeButtonInstance == null)
            return;

        bool canConsume = currentItem.isConsumable;
        consumeButtonInstance.interactable = canConsume;

        consumeButtonInstance.gameObject.SetActive(!consumeButtonInstance.gameObject.activeSelf);

        // Notificar al InventoryUI cuál es el botón activo
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.RegisterActiveConsumeButton(consumeButtonInstance);
    }

    private void OnConsumeClicked()
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

        consumeButtonInstance.gameObject.SetActive(false);
    }

    public void HideConsumeButton()
    {
        if (consumeButtonInstance != null)
            consumeButtonInstance.gameObject.SetActive(false);
    }

    public Image IconImage => iconImage;
}
