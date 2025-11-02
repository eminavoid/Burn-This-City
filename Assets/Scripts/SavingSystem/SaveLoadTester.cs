using UnityEngine;

/// <summary>
/// Script temporal de prueba para guardar y cargar la partida.
/// Presiona 'O' para guardar.
/// Presiona 'P' para cargar.
/// </summary>
public class SaveLoadTester : MonoBehaviour
{
    void Update()
    {
        // --- GUARDAR ---
        // Usamos GetKeyDown para que solo se dispare una vez por pulsación
        if (Input.GetKeyDown(KeyCode.O))
        {
            if (SaveManager.Instance != null)
            {
                Debug.LogWarning("--- [PRUEBA] Iniciando GUARDADO con 'O' ---");
                SaveManager.Instance.SaveGame();
            }
            else
            {
                Debug.LogError("No se encontró SaveManager.Instance al intentar guardar.");
            }
        }

        // --- CARGAR ---
        if (Input.GetKeyDown(KeyCode.P))
        {
            if (SaveManager.Instance != null)
            {
                Debug.LogWarning("--- [PRUEBA] Iniciando CARGA con 'P' ---");
                SaveManager.Instance.LoadGame();
            }
            else
            {
                Debug.LogError("No se encontró SaveManager.Instance al intentar cargar.");
            }
        }
    }
}