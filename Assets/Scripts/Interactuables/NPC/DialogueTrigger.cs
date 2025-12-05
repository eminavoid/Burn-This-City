using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("ID")] [SerializeField] private int npcID;
    
    [Header("Initial Dialogue")]
    [Tooltip("The DialogueNode to start with on first interaction.")]
    public DialogueNode startingNode;

    private DialogueNode currentNode;
    private DialogueRunner runner;

    [Header("Signals from dialogue (scene events)")]
    public UnityEvent OnTalked;
    public UnityEvent OnSucces;
    public UnityEvent OnFailure;

    private bool hasTalked;
    private bool hasSucceeded;
    private bool hasFailed;

    [Header("Persistence Settings")]
    [Tooltip("Si es true, al cargar la partida se volverán a disparar los eventos (ej. OnSuccess) si ya ocurrieron. Útil para mantener puertas abiertas, objetos borrados, etc.")]
    [SerializeField] private bool triggerEventsOnLoad = true;

    [SerializeField] public GameObject icon;

    [Tooltip("Si es TRUE, este NPC NUNCA actualizará el SaveManager.")]
    [SerializeField] private bool disableAutoSave = false;

    public string InteractionPrompt => "Talk";
    public bool CanInteract(StatManager stats) => true;

    private void Awake()
    {
        runner = DialogueRunner.FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
            Debug.LogError($"{name}: No DialogueRunner instance found!");

        var savedState = SaveManager.Instance?.GetNpcNode(npcID);
        if (savedState != null)
        {
            // Restaurar nodo
            currentNode = savedState.currentNode;

            // Restaurar flags
            hasTalked = savedState.hasTalked;
            hasSucceeded = savedState.hasSucceeded;
            hasFailed = savedState.hasFailed;

            // OPCIONAL: Re-disparar eventos si queremos persistencia visual en el mundo
            if (triggerEventsOnLoad)
            {
                if (hasTalked) OnTalked?.Invoke();
                if (hasSucceeded) OnSucces?.Invoke();
                if (hasFailed) OnFailure?.Invoke();
            }
        }
        else
        {
            // Estado inicial por defecto
            currentNode = startingNode;
            hasTalked = false;
            hasSucceeded = false;
            hasFailed = false;

            UpdateSaveManager();
        }
    }

    private void UpdateSaveManager()
    {
        if (disableAutoSave)
        {
            Debug.Log($"Autoguardado prevenido para el NPC: {name}");
            return;
        }
        SaveManager.Instance?.UpdateNpcState(npcID, currentNode, hasTalked, hasSucceeded, hasFailed);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (icon != null) icon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (icon != null) icon.SetActive(false);
        }
    }

    public void Interact(StatManager stats)
    {
        if (currentNode != null)
            runner.Begin(currentNode, this);
        else
            Debug.LogWarning($"{name}: currentNode is null (did you set startingNode?).");
    }

    public void SetStartingNode(DialogueNode newNode)
    {
        currentNode = newNode;
        UpdateSaveManager();
    }

    // Llamado por el runner cuando un choice marca el flag
    public void RaiseOnTalked()
    {
        hasTalked = true;
        OnTalked?.Invoke();
        UpdateSaveManager();
    }
    public void RaiseOnSucces()
    {
        hasSucceeded = true;
        OnSucces?.Invoke();
        UpdateSaveManager() ;
    }
    public void RaiseOnFailure()
    {
        hasFailed = true;
        OnFailure?.Invoke();
        UpdateSaveManager() ;
    }
}
