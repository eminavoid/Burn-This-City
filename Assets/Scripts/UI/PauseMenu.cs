using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;
    
    
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))   
        {
            TogglePause();
        }
    }

    public void ResumeButton()
    {
        container.SetActive(false);
        Time.timeScale = 1;
    }
    
    public void MenuButton()
    {
        container.SetActive(false);
        Time.timeScale = 1f;
        SceneManager.LoadScene("Main Menu");
    }

    private void TogglePause()
    {
        if (container.activeSelf)
        {
            container.SetActive(false);
            Time.timeScale = 1f;
        }
        else
        {
            container.SetActive(true);
            Time.timeScale = 1;
        }
    }
}
