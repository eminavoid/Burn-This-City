using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class DialogueChoice
{
    [TextArea(1, 2)] public string playerText;
    public DialogueNode nextNode;
    public List<DialogueReward> rewards;
}