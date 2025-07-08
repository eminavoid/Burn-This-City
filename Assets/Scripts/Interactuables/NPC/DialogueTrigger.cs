using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Header("Initial Dialogue")]
    [Tooltip("The DialogueNode to start with on first interaction.")]
    public DialogueNode startingNode;

    private DialogueNode currentNode;
    private DialogueRunner runner;

    public string InteractionPrompt => "Talk";
    public bool CanInteract(StatManager stats) => true;

    private void Awake()
    {
        runner = DialogueRunner.FindAnyObjectByType<DialogueRunner>();
        if (runner == null)
            Debug.LogError($"{name}: No DialogueRunner instance found!");

        currentNode = startingNode;
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
    }
}
