using UnityEngine;

public class NewGameButtonBehaviour : MonoBehaviour
{
    public void ResetStatsAndLoadScene(string sceneToLoad)
    {
        StatManager.Instance.ResetAllStats();
        SceneController.Instance.LoadScene(sceneToLoad);
    }
}
