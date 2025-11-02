using UnityEngine;

public class NewGameButtonBehaviour : MonoBehaviour
{
    public void StartNewGame(string sceneToLoad)
    {
        Time.timeScale = 1;
        Debug.Log("--- INICIANDO NUEVA PARTIDA ---");

        // --- 1. Borrar datos guardados en disco ---
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.DeleteSavedData();
        }

        // --- 2. Resetear todos los managers en memoria ---

        if (StatManager.Instance != null)
            StatManager.Instance.ResetAllStats();

        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.InitModules();
            InventoryManager.Instance.ForceRefresh();
        }

        if (SurvivabilityManager.Instance != null)
            SurvivabilityManager.Instance.ResetStatsToDefault();

        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.ResetSessionData();
        }

        // --- 3. Cargar la escena inicial ---
        Debug.Log($"Cargando escena de nueva partida: {sceneToLoad}");
        if (SceneController.Instance != null)
            SceneController.Instance.LoadScene(sceneToLoad);
        else
            Debug.LogError("NewGameButton: SceneController.Instance no encontrado.");
    }
}