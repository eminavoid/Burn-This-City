using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NewDialogueNode", menuName = "Dialogue/Node")]
public class DialogueNode : ScriptableObject
{
    [Header("Who’s talking?")]
    public string npcName;
    [TextArea(2, 6)] public string npcText;
    public List<DialogueChoice> choices = new List<DialogueChoice>();
}