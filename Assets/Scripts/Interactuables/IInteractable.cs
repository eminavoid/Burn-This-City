public interface IInteractable
{
    string InteractionPrompt { get; }
    bool CanInteract(StatManager statManager);
    void Interact(StatManager statManager);
}