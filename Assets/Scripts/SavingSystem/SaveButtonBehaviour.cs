using UnityEngine;

public class SaveButtonBehaviour : MonoBehaviour
{

    public void OnSaveButtonPressed()
    {
        if (SaveManager.Instance != null)
            SaveManager.Instance.SaveGame();
        else
            Debug.LogError("SaveButton: No se encontró SaveManager.Instance al intentar guardar.");
    }
}