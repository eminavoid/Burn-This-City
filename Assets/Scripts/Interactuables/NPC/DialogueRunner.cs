using UnityEngine;
using System.Linq;
using System;

public class DialogueRunner : MonoBehaviour
{
    public DialogueUI ui;
    private DialogueNode current;

    public static event Action DialogueStarted;
    public static event Action DialogueEnded;

    public void Begin(DialogueNode start)
    {
        DialogueStarted?.Invoke();

        current = start;
        Advance();
    }

    private void Advance()
    {
        if (current == null)
        {
            ui.Hide();
            DialogueEnded?.Invoke();
            return;
        }

        var options = current.choices
                             .Select((c, i) => (c.playerText, i))
                             .ToList();
        ui.Show(current.npcName, current.npcText, options);
    }
    public void Choose(int index)
    {
        var choice = current.choices[index];

        //Always-grant rewards
        if (choice.grantedReward != null)
            foreach (var r in choice.grantedReward)
                StatManager.Instance.IncrementStat(r.statType, r.amount);

        //Check conditional requirements
        bool hasConds = choice.statRequirements != null && choice.statRequirements.Count > 0;
        if (hasConds)
        {
            bool success = choice.statRequirements
                                .All(req => req.IsMet(StatManager.Instance.GetStat(req.statType)));

            if (success)
            {
                // success branch
                if (choice.successRewards != null)
                    foreach (var r in choice.successRewards)
                        StatManager.Instance.IncrementStat(r.statType, r.amount);

                choice.onSuccess?.Invoke();
                current = choice.successNode;
            }
            else
            {
                // failure branch
                if (choice.failureRewards != null)
                    foreach (var r in choice.failureRewards)
                        StatManager.Instance.IncrementStat(r.statType, r.amount);

                choice.onFailure?.Invoke();
                current = choice.failureNode;
            }
        }
        else
        {
            // no conditions
            current = choice.defaultNode;
        }

        Advance();
    }
}