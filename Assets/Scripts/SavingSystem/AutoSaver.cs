using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class AutoSaver : MonoBehaviour
{
    public static AutoSaver Instance { get; private set; }

    [Header("Configuración")]
    [Tooltip("Tiempo de espera para guardar tras entrar a una escena (para evitar tirones durante el fade-in).")]
    [SerializeField] private float autoSaveDelay = 1.0f;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ShouldAutoSave())
        {
            StartCoroutine(AutoSaveWithDelay());
        }
    }

    private bool ShouldAutoSave()
    {
        if (SaveManager.Instance == null) return false;

        string currentScene = SceneManager.GetActiveScene().name;

        if (SaveManager.Instance.noAutoSaveScenes.Contains(currentScene))
        {
            return false;
        }

        return true;
    }

    private IEnumerator AutoSaveWithDelay()
    {
        yield return new WaitForSecondsRealtime(autoSaveDelay);

        Debug.Log("[AutoSaver] Sobrescribiendo partida (Estilo Dark Souls)...");

        SaveManager.Instance.SaveGame();
    }

    public void TriggerAutoSave()
    {
        if (ShouldAutoSave())
        {
            Debug.Log("[AutoSaver] Guardado activado por evento");

            if (SaveManager.Instance != null)
            {
                SaveManager.Instance.SaveGame();

            }
        }
    }
}