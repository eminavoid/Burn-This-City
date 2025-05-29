// InteractableData.cs
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "InteractableData", menuName = "Interactables/Interactable Data")]
public class InteractableData : ScriptableObject
{
    public enum InteractionType
    {
        NPC,
        SimpleObject,
        Door,
        Chest,
        StatUpgrade
    }

    public InteractionType interactionType;
    public List<InteractionRequirement> requirements;
    [TextArea]
    public string interactionPrompt = "Interactuar";
}