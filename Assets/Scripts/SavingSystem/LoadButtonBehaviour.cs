using UnityEngine;

public class LoadButtonBehaviour : MonoBehaviour
{
    public void OnLoadButtonPressed()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.LoadGame();
        else
            Debug.LogError("LoadButton: No se encontró SaveManager.Instance al intentar cargar.");
    }
}