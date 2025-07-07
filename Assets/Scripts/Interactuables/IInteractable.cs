public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract(StatManager stats);
    void Interact(StatManager stats);
}