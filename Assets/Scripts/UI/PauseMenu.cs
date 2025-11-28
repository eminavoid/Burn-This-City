using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    public GameObject container;
    public GameObject inventoryUI;

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            HandleEscape();
        }
    }

    private void HandleEscape()
    {
        if (inventoryUI.activeSelf)
        {
            InventoryUI.Instance.ToggleInventory();
            return;
        }

        TogglePause();
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
            Time.timeScale = 0f;
        }
    }
}
