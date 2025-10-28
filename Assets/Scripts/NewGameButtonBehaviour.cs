using UnityEngine;

public class NewGameButtonBehaviour : MonoBehaviour
{
    public void ResetStatsAndLoadScene(string sceneToLoad)
    {
        Time.timeScale = 1;
        StatManager.Instance.ResetAllStats();
        SceneController.Instance.LoadScene(sceneToLoad);
    }
}
