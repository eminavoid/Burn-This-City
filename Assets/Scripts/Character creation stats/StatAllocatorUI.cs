using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatAllocatorUI : MonoBehaviour
{
    [Header("Configuración")]
    [SerializeField] private int totalPointsToAssign = 18;

    [Header("Referencias de UI")]
    [SerializeField] private TMP_Text pointsRemainingText;
    [SerializeField] private Button startGameButton;

    [SerializeField] private List<StatAllocatorEntry> statEntries;

    [SerializeField] private string gameScene;
    [SerializeField] private string menuScene;

    private void OnEnable()
    {
        StatManager.OnStatChanged += OnStatChangedCallback;

        UpdateUI();
    }
    private void OnDisable()
    {
        StatManager.OnStatChanged -= OnStatChangedCallback;
    }
    private void OnStatChangedCallback(StatManager.StatType type, int newValue)
    {
        UpdateUI();
    }
    private void UpdateUI()
    {
        if (StatManager.Instance == null) return;

        int totalSpent = 0;

        foreach (var entry in statEntries)
        {
            StatManager.StatType type = entry.GetStatType();
            int currentValue = StatManager.Instance.GetStat(type);

            totalSpent += (currentValue - 1);
        }

        int pointsRemaining = totalPointsToAssign - totalSpent;
        pointsRemainingText.text = $"Puntos Restantes: {pointsRemaining}";

        startGameButton.interactable = (pointsRemaining == 0);

        foreach (var entry in statEntries)
        {
            entry.Refresh(pointsRemaining);
        }
    }
    public void StartGame()
    {
        if (startGameButton.interactable)
        {
            Debug.Log("Iniciando el juego..."); 
            SceneController.Instance.LoadScene(gameScene);
        }
    }
    public void ReturnToMenu()
    {
        SceneController.Instance.LoadScene(menuScene);
    }
}
