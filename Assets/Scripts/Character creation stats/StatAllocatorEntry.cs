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

    [Header("Settings")]
    [SerializeField] private int maxStatValue = 12;   
    [SerializeField] private int minStatValue = 1;    

    private void Start()
    {
        statNameText.text = statType.ToString() + ":";

        incrementButton.onClick.AddListener(OnIncrement);
        decrementButton.onClick.AddListener(OnDecrement);
    }

    private void OnIncrement()
    {
        if (StatManager.Instance.GetStat(statType) < maxStatValue)
        {
            StatManager.Instance.IncrementStat(statType, 1);
        }
    }

    private void OnDecrement()
    {
        if (StatManager.Instance.GetStat(statType) > minStatValue)
        {
            StatManager.Instance.IncrementStat(statType, -1);
        }
    }

    public StatManager.StatType GetStatType()
    {
        return statType;
    }

    public int GetMinValue()     
    {
        return minStatValue;
    }

    public void Refresh(int pointsRemaining)
    {
        int currentValue = StatManager.Instance.GetStat(statType);
        statValueText.text = currentValue.ToString();

        incrementButton.interactable = (pointsRemaining > 0) && (currentValue < maxStatValue);

        decrementButton.interactable = (currentValue > minStatValue);
    }
}