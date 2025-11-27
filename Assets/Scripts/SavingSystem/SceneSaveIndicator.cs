using UnityEngine;

public class SceneSaveIndicator : MonoBehaviour
{
    private void Start()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavingIndicator(this.gameObject);
        }
    }

    private void OnDestroy()
    {
        if (SaveManager.Instance != null)
        {
            SaveManager.Instance.RegisterSavingIndicator(null);
        }
    }
} 