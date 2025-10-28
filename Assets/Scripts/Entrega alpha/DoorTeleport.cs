using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class DoorTeleport : MonoBehaviour, IInteractable
{
    [Header("Configuración de la puerta")]
    [SerializeField] private bool requiresKey = true;
    [SerializeField] private bool useSceneTransition = false;
    [SerializeField] private string sceneToLoad;
    [SerializeField] private Transform teleportDestination;
    
    [Header("Spawn personalizado (solo si cambia de escena)")]
    [SerializeField] private bool useCustomSpawn = false;
    [SerializeField] private string targetSpawnPointName;
    
    [Header("Mensajes personalizables")]
    [SerializeField] private string interactPrompt = "F para interactuar";
    [SerializeField] private string lockedPrompt = "Necesitás una llave";

    [Header("UI")]
    [SerializeField] private GameObject promptUI; // Objeto con el texto encima
    [SerializeField] private TextMeshProUGUI promptText;

    private bool playerInRange = false;
    private PlayerMovement2D playerInteraction;

    public string InteractionPrompt => "Walk";
    public bool CanInteract(StatManager stats) => true;

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

    public void Interact(StatManager stats)
    {
            if (!requiresKey || (playerInteraction != null && playerInteraction.hasKey))
            {
                if (useSceneTransition && !string.IsNullOrEmpty(sceneToLoad))
                {
                    Debug.Log($"Cargando escena: {sceneToLoad}");

                    SceneSpawnManager.NextSpawnPoint = useCustomSpawn ? targetSpawnPointName : null;
                    
                    ScreenFader.Instance.FadeOutAndLoadScene(sceneToLoad);
                }
                else if (teleportDestination != null)
                {
                    Teleport(teleportDestination);
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

    private void UpdatePromptText()
    {
        if (promptText == null || playerInteraction == null) return;

        promptText.text = (!requiresKey || playerInteraction.hasKey)
            ? interactPrompt
            : lockedPrompt;
    }

    private void Teleport(Transform tpDestination)
    {
        playerInteraction.transform.position = tpDestination.position;
        Debug.Log("¡Teletransportado!");
    }

    private void LateUpdate()
    {
        if (playerInRange)
            UpdatePromptText();
    }
}
