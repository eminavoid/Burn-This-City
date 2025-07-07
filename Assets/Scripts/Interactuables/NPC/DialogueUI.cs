using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Shows the NPC’s name")]
    public TMP_Text npcNameText;

    [Tooltip("Shows what the NPC actually says")]
    public TMP_Text dialogueText;

    [Tooltip("Exactly 3 buttons here")]
    public Button[] choiceButtons; // size = 3

    public void Show(string npcName, string text, List<(string optionText, int optionIndex)> options)
    {
        gameObject.SetActive(true);
        npcNameText.text = npcName;
        dialogueText.text = text;

        for (int i = 0; i < choiceButtons.Length; i++)
        {
            var btn = choiceButtons[i];
            bool has = i < options.Count;
            btn.gameObject.SetActive(has);
            btn.onClick.RemoveAllListeners();

            if (has)
            {
                var label = btn.GetComponentInChildren<TMP_Text>();
                label.text = options[i].optionText;

                int idx = options[i].optionIndex;
                btn.onClick.AddListener(() => 
                {
                    var runner = FindFirstObjectByType<DialogueRunner>();
                    runner?.Choose(idx);
                });
            }
        }
    }
    public void Hide() => gameObject.SetActive(false);
}