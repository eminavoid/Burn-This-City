using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [Tooltip("Root DialogueNode for this NPC")]
    public DialogueNode startingNode;

    private DialogueRunner runner;

    private void Awake()
    {
        runner = Object.FindFirstObjectByType<DialogueRunner>();
        if (runner == null)
            Debug.LogError("DialogueRunner missing in scene!");
    }

    public string InteractionPrompt => "Talk";
    public bool CanInteract(StatManager stats) => true;

    public void Interact(StatManager stats)
    {
        runner.Begin(startingNode);
    }
}