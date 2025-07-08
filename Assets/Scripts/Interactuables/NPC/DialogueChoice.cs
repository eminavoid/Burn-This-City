using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 2)] public string playerText;

    [Header("Default with no requirement")]
    public DialogueNode defaultNode;
    public List<DialogueReward> grantedReward;
    [Tooltip("If set, the NPC’s startingNode will be replaced with this after choosing.")]
    public DialogueNode nextStartingNodeDefault;
    public UnityEvent onTalked;

    [Header("Conditional requirements")]
    public List<InteractionRequirement> statRequirements;

    [Header("Success")]
    public DialogueNode successNode;
    public List<DialogueReward> successRewards;
    [Tooltip("If set, NPC will start here next time (success branch)")]
    public DialogueNode nextStartingNodeSuccess;
    public UnityEvent onSuccess;

    [Header("Fail")]
    public DialogueNode failureNode;
    public List<DialogueReward> failureRewards;
    [Tooltip("If set, NPC will start here next time (failure branch)")]
    public DialogueNode nextStartingNodeFailure;
    public UnityEvent onFailure;
}
