using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatAllocatorEntry : MonoBehaviour
{
    [SerializeField] private StatManager.StatType statType;

    [SerializeField] private TMP_Text statNameText;
    [SerializeField] private TMP_Text statValueText;
    [SerializeField] private Button incrementButton;
    [SerializeField] private Button decrementButton;

    private void Start()
    {
        // Asigna el nombre de la stat automáticamente
        statNameText.text = statType.ToString()+":";

        // Conecta los listeners de los botones
        incrementButton.onClick.AddListener(OnIncrement);
        decrementButton.onClick.AddListener(OnDecrement);
    }
    private void OnIncrement()
    {
        StatManager.Instance.IncrementStat(statType, 1);
    }

    private void OnDecrement()
    {
        StatManager.Instance.IncrementStat(statType, -1);
    }
    public StatManager.StatType GetStatType()
    {
        return statType;
    }
    public void Refresh(int pointsRemaining)
    {
        int currentValue = StatManager.Instance.GetStat(statType);
        statValueText.text = currentValue.ToString();

        incrementButton.interactable = (pointsRemaining > 0);

        decrementButton.interactable = (currentValue > 1);
    }
}
