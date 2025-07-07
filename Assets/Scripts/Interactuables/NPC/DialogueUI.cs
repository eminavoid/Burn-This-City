using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text npcText;
    public TMP_Text npcName;
    public Button[] choiceButtons; // size must be 3

    public void Show(string text, List<(string optionText, int optionIndex)> options)
    {
        gameObject.SetActive(true);
        npcText.text = text;

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
                btn.onClick.AddListener(() => {
                    var runner = FindFirstObjectByType<DialogueRunner>();
                    runner?.Choose(idx);
                });
            }
        }
    }
    public void Hide() => gameObject.SetActive(false);
}