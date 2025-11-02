using UnityEngine;

public class ButtonLoadScene : MonoBehaviour
{
    public void OnClickLoadScene(string sceneToLoad)
    {
        if (SceneController.Instance != null)
            SceneController.Instance.LoadScene(sceneToLoad);
        else
            Debug.LogError("NewGameButton: SceneController.Instance no encontrado.");
    }
}
