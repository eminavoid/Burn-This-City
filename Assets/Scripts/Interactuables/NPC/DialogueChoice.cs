using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 2)] public string playerText;

    [Header("Default with no requirement")]
    public DialogueNode defaultNode;
    public List<DialogueReward> grantedReward;
    [Tooltip("Item rewards (default branch)")]
    public List<ItemAmount> defaultItemRewards;
    [Tooltip("If set, the NPC’s startingNode will be replaced with this after choosing.")]
    public DialogueNode nextStartingNodeDefault;

    [Tooltip("If true, the DialogueTrigger will raise its scene event (OnTalked) when this default branch is chosen.")]
    public bool raiseOnTalked;

    [Header("Conditional requirements")]
    public List<InteractionRequirement> statRequirements;
    [Tooltip("Optional item requirements (e.g., entregar ingredientes)")]
    public List<ItemAmount> itemRequirements;
    [Tooltip("Si true, se consumen los items requeridos al cumplir la condición (success)")]
    public bool consumeRequirementsOnSuccess = true;

    [Header("Success")]
    public DialogueNode successNode;
    public List<DialogueReward> successRewards;
    [Tooltip("Item rewards (success branch)")]
    public List<ItemAmount> successItemRewards;
    [Tooltip("If set, NPC will start here next time (success branch)")]
    public DialogueNode nextStartingNodeSuccess;

    [Tooltip("If true, raise OnTalked on SUCCESS.")]
    public bool raiseOnSuccess;

    [Header("Fail")]
    public DialogueNode failureNode;
    public List<DialogueReward> failureRewards;
    [Tooltip("Item rewards (failure branch)")]
    public List<ItemAmount> failureItemRewards;
    [Tooltip("If set, NPC will start here next time (failure branch)")]
    public DialogueNode nextStartingNodeFailure;

    [Tooltip("If true, raise OnTalked on FAILURE.")]
    public bool raiseOnFailure;
}
