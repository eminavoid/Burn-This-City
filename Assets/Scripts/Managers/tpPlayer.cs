using UnityEngine;

public class tpPlayer : MonoBehaviour
{
    public void loadSceneName(string scene)
    {
        SceneController.Instance.LoadScene(scene);
    }
    public void useFade(string scene)
    {
        ScreenFader.Instance.FadeOutAndLoadScene(scene);
    }
}
