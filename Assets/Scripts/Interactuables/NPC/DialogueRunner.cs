using UnityEngine;
using System.Linq;
using System;
using UnityEngine.SceneManagement;

public class DialogueRunner : MonoBehaviour
{
    public static DialogueRunner Instance { get; private set; }

    [SerializeField] private DialogueUI ui;
    private DialogueNode current;
    private DialogueTrigger currentTrigger;

    public static event Action DialogueStarted;
    public static event Action DialogueEnded;

    
    public void Begin(DialogueNode start, DialogueTrigger trigger)
    {
        currentTrigger = trigger;
        DialogueStarted?.Invoke();
        current = start;
        Advance();
    }

    private void Advance()
    {
        if (current == null || current.choices == null)
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

        // no stat requirements
        if (choice.statRequirements == null || choice.statRequirements.Count == 0)
        {
            if (choice.grantedReward != null)
                foreach (var r in choice.grantedReward)
                    StatManager.Instance.IncrementStat(r.statType, r.amount);

            choice.onTalked?.Invoke();

            if (choice.nextStartingNodeDefault != null && currentTrigger != null)
                currentTrigger.SetStartingNode(choice.nextStartingNodeDefault);

            current = choice.defaultNode;
        }
        else
        {
            // Conditional branch 
            bool success = choice.statRequirements
                                 .All(req => req.IsMet(StatManager.Instance.GetStat(req.statType)));

            if (success)
            {
                if (choice.successRewards != null)
                    foreach (var r in choice.successRewards)
                        StatManager.Instance.IncrementStat(r.statType, r.amount);

                choice.onSuccess?.Invoke();

                if (choice.nextStartingNodeSuccess != null && currentTrigger != null)
                    currentTrigger.SetStartingNode(choice.nextStartingNodeSuccess);

                current = choice.successNode;
            }
            else
            {
                if (choice.failureRewards != null)
                    foreach (var r in choice.failureRewards)
                        StatManager.Instance.IncrementStat(r.statType, r.amount);

                choice.onFailure?.Invoke();

                if (choice.nextStartingNodeFailure != null && currentTrigger != null)
                    currentTrigger.SetStartingNode(choice.nextStartingNodeFailure);

                current = choice.failureNode;
            }
        }

        Advance();
    }
}