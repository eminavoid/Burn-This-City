using UnityEngine;
using System.Collections.Generic;
using UnityEngine.Events;

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 2)] public string playerText;

    [Header("Default with no requirement")]
    public DialogueNode defaultNode;

    [Header("Always-granted rewards")]
    public List<DialogueReward> grantedReward;

    [Header("Conditional requirements")]
    public List<InteractionRequirement> statRequirements;

    [Header("Success")]
    public DialogueNode successNode;
    public List<DialogueReward> successRewards;
    public UnityEvent onSuccess;

    [Header("Fail")]
    public DialogueNode failureNode;
    public List<DialogueReward> failureRewards;
    public UnityEvent onFailure;
}
