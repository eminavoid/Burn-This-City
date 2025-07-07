using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class DialogueTrigger : MonoBehaviour, IInteractable
{
    [SerializeField] private DialogueNode startingNode;
    private DialogueRunner runner;

    private void Awake()
    {
        runner = FindAnyObjectByType<DialogueRunner>();
        if (runner == null) 
            Debug.LogError("No DialogueRunner in scene.");
    }
    public string InteractionPrompt => "Talk";
    public bool CanInteract(StatManager stat) => true;
    public void Interact(StatManager stat)
    {
        runner.Begin(startingNode);
    }
}