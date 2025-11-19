using System;
using UnityEngine;

public class ExitButton : MonoBehaviour
{
    public void OnClickExitGame()
    {
        if (SceneController.Instance != null)
        {
            Time.timeScale = 1f;
            SceneController.Instance.ExitGame();
        } else
            Debug.LogError("NewGameButton: SceneController.Instance no encontrado.");
    }
}
