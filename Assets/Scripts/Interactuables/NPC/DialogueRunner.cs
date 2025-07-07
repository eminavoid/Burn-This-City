using UnityEngine;
using System.Linq;

public class DialogueRunner : MonoBehaviour
{
    public DialogueUI ui;
    private DialogueNode current;

    public void Begin(DialogueNode start)
    {
        current = start;
        Advance();
    }

    private void Advance()
    {
        if (current == null)
        {
            ui.Hide();
            return;
        }

        var options = current.choices
                             .Select((c, i) => (c.playerText, i))
                             .ToList();
        ui.Show(current.npcText, options);
    }
    public void Choose(int index)
    {
        var choice = current.choices[index];
        if (choice.rewards != null)
            foreach (var r in choice.rewards)
                StatManager.Instance.IncrementStat(r.statType, r.amount);

        current = choice.nextNode;
        Advance();
    }
}