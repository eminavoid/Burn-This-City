using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTeleport : MonoBehaviour
{
    [Header("Configuración de la puerta")]
    [SerializeField] private bool requiresKey = true;
    [SerializeField] private bool useSceneTransition = false;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Transform teleportDestination;
    
    [Header("Mensajes personalizables")]
    [SerializeField] private string interactPrompt = "F para interactuar";
    [SerializeField] private string lockedPrompt = "Necesitás una llave";

    [Header("UI")]
    [SerializeField] private GameObject promptUI; // Objeto con el texto encima
    [SerializeField] private TextMeshProUGUI promptText;

    private bool playerInRange = false;
    private PlayerMovement2D playerInteraction;

    private void Start()
    {
        if (promptUI != null)
            promptUI.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInteraction = other.GetComponent<PlayerMovement2D>();
            if (playerInteraction != null)
            {
                playerInRange = true;
                UpdatePromptText();
                promptUI.SetActive(true);
            }
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerInteraction = null;
            playerInRange = false;
            if (promptUI != null)
                promptUI.SetActive(false);
        }
    }

    private void Update()
    {
        if (playerInRange && Input.GetKeyDown(KeyCode.F))
        {
            if (!requiresKey || (playerInteraction != null && playerInteraction.hasKey))
            {
                if (useSceneTransition && !string.IsNullOrEmpty(sceneToLoad))
                {
                    Debug.Log($"Cargando escena: {sceneToLoad}");
                    ScreenFader.Instance.FadeOutAndLoadScene(sceneToLoad);
                }
                else if (teleportDestination != null)
                {
                    playerInteraction.transform.position = teleportDestination.position;
                    Debug.Log("¡Teletransportado!");
                }
                else
                {
                    Debug.LogWarning("No hay destino de teletransporte asignado.");
                }
            }
            else
            {
                Debug.Log("La puerta está cerrada. Necesitás una llave.");
            }
        }
    }

    private void UpdatePromptText()
    {
        if (promptText == null || playerInteraction == null) return;

        if (!requiresKey || playerInteraction.hasKey)
        {
            promptText.text = interactPrompt;
        }
        else
        {
            promptText.text = lockedPrompt;
        }
    }

    private void LateUpdate()
    {
        if (playerInRange)
            UpdatePromptText();
    }
}
