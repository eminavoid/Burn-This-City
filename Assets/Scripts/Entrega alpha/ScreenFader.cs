using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using UnityEngine.SceneManagement;

public class ScreenFader : MonoBehaviour
{
    public Image fadeImage;
    [SerializeField] private float fadeDuration = 1f;

    public static ScreenFader Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // persiste entre escenas
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        // Asegura que comience con fade-in al cargar la primera escena
        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (fadeImage != null)
            StartCoroutine(FadeIn());
    }

    public void FadeOutAndLoadScene(string sceneName)
    {
        StartCoroutine(FadeOutAndLoad(sceneName));
    }

    private IEnumerator FadeOutAndLoad(string sceneName)
    {
        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        float t = 0;

        // Fade Out
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Cargar nueva escena
        SceneManager.LoadScene(sceneName);
    }

    private IEnumerator FadeIn()
    {
        fadeImage.gameObject.SetActive(true);

        Color color = fadeImage.color;
        color.a = 1f;
        fadeImage.color = color;

        float t = 0;
        while (t < fadeDuration)
        {
            t += Time.deltaTime;
            color.a = 1f - Mathf.Clamp01(t / fadeDuration);
            fadeImage.color = color;
            yield return null;
        }

        // Ocultar la imagen si ya no es necesaria
        fadeImage.gameObject.SetActive(false);
    }
}
