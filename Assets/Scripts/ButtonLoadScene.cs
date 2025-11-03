using UnityEngine;

public class ButtonLoadScene : MonoBehaviour
{
    public void OnClickLoadScene(string sceneToLoad)
    {
        if (SceneController.Instance != null)
        {
            Time.timeScale = 1f;
            SceneController.Instance.LoadScene(sceneToLoad);
        } else
            Debug.LogError("NewGameButton: SceneController.Instance no encontrado.");
    }
}
