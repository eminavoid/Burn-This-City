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

    [SerializeField] public GameObject icon;

    public string InteractionPrompt => "Talk";
    public bool CanInteract(StatManager stats) => true;

    private void Awake()
    {
        runner = DialogueRunner.FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
            Debug.LogError($"{name}: No DialogueRunner instance found!");

        // Consultamos al SaveManager si hay un estado guardado para este NPC
        var savedNode = SaveManager.Instance?.GetNpcNode(npcID);
        if (savedNode != null)
        {
            currentNode = savedNode;
        }
        else
        {
            // Si no hay nada guardado, usamos el de inspector
            currentNode = startingNode;
            // Y registramos este estado inicial en el manager
            SaveManager.Instance?.UpdateNpcNode(npcID, currentNode);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            icon.SetActive(true);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            icon.SetActive(false);
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
        SaveManager.Instance?.UpdateNpcNode(npcID, currentNode);
    }

    // Llamado por el runner cuando un choice marca el flag
    public void RaiseOnTalked()
    {
        OnTalked?.Invoke();
    }
    public void RaiseOnSucces()
    {
        OnSucces?.Invoke();
    }
    public void RaiseOnFailure()
    {
        OnFailure?.Invoke();
    }
}
