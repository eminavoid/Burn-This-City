using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class StatDisplayUI : MonoBehaviour
{
    [SerializeField]
    private GameObject statTextPrefab;

    [SerializeField]
    private Transform statContainer;

    private Dictionary<StatManager.StatType, TextMeshProUGUI> statTextDisplays = new Dictionary<StatManager.StatType, TextMeshProUGUI>();

    private void Start()
    {
        if (StatManager.Instance == null)
        {
            Debug.LogError("StatManager no encontrado. Asegúrate de que esté en la escena.");
            return;
        }

        InitializeUI();

        StatManager.OnStatChanged += HandleStatChange;
    }

    private void OnDestroy()
    {
        StatManager.OnStatChanged -= HandleStatChange;
    }

    private void InitializeUI()
    {
        foreach (Transform child in statContainer)
        {
            Destroy(child.gameObject);
        }
        statTextDisplays.Clear();

        foreach (StatManager.StatType statType in System.Enum.GetValues(typeof(StatManager.StatType)))
        {
            GameObject statInstance = Instantiate(statTextPrefab, statContainer);
            TextMeshProUGUI textComponent = statInstance.GetComponent<TextMeshProUGUI>();

            if (textComponent != null)
            {
                int currentValue = StatManager.Instance.GetStat(statType);

                textComponent.text = $"{statType.ToString()}: {currentValue}";

                statTextDisplays.Add(statType, textComponent);
            }
            else
            {
                Debug.LogError($"El prefab 'statTextPrefab' no tiene un componente TextMeshProUGUI.", this);
            }
        }
    }

    private void HandleStatChange(StatManager.StatType statType, int newValue)
    {
        if (statTextDisplays.TryGetValue(statType, out TextMeshProUGUI textComponent))
        {
            textComponent.text = $"{statType.ToString()}: {newValue}";
        }
    }
}
