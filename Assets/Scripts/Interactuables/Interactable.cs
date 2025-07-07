// Interactable.cs
using UnityEngine;

public class Interactable : MonoBehaviour, IInteractable
{
    public InteractableData interactableData;

    public string InteractionPrompt => interactableData.interactionPrompt;

    public virtual bool CanInteract(StatManager statManager)
    {
        if (interactableData.requirements == null || interactableData.requirements.Count == 0)
        {
            return true;
        }

        foreach (InteractionRequirement requirement in interactableData.requirements)
        {
            int currentStat = statManager.GetStat(requirement.statType);
            if (!requirement.IsMet(currentStat))
            {
                return false;
            }
        }

        return true;
    }

    public virtual void Interact(StatManager statManager)
    {
        Debug.Log($"Interactuando con {gameObject.name} (Tipo: {interactableData.interactionType})");

        // BASIC INTERACTION LOGIC IF ANY
    }

}