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

        bool hasStatReqs = choice.statRequirements != null && choice.statRequirements.Count > 0;
        bool hasItemReqs = choice.itemRequirements != null && choice.itemRequirements.Count > 0;

        // ——— Caso sin requisitos: solo dar recompensas default ———
        if (!hasStatReqs && !hasItemReqs)
        {
            // Stats
            if (choice.grantedReward != null)
                foreach (var r in choice.grantedReward)
                    StatManager.Instance.IncrementStat(r.statType, r.amount);

            // Items
            if (choice.defaultItemRewards != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddMany(choice.defaultItemRewards);
                InventoryUI.RefreshIfAnyOpen(); 
            }

            choice.onTalked?.Invoke();

            if (choice.nextStartingNodeDefault != null && currentTrigger != null)
                currentTrigger.SetStartingNode(choice.nextStartingNodeDefault);

            current = choice.defaultNode;
            Advance();
            return;
        }

        // ——— Caso con requisitos: chequear stats + items ———
        bool statsOk = true;
        if (hasStatReqs)
            statsOk = choice.statRequirements.All(req => req.IsMet(StatManager.Instance.GetStat(req.statType)));

        bool itemsOk = true;
        if (hasItemReqs)
            itemsOk = InventoryManager.Instance != null && InventoryManager.Instance.CanConsume(choice.itemRequirements);

        bool success = statsOk && itemsOk;

        if (success)
        {
            // Consumir requisitos de items si corresponde (ej: "entregar ingredientes")
            if (hasItemReqs && choice.consumeRequirementsOnSuccess && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Consume(choice.itemRequirements);
                InventoryUI.RefreshIfAnyOpen();
            }

            // Recompensas de éxito (stats)
            if (choice.successRewards != null)
                foreach (var r in choice.successRewards)
                    StatManager.Instance.IncrementStat(r.statType, r.amount);

            // Recompensas de éxito (items)
            if (choice.successItemRewards != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddMany(choice.successItemRewards);
                InventoryUI.RefreshIfAnyOpen();
            }

            choice.onSuccess?.Invoke();

            if (choice.nextStartingNodeSuccess != null && currentTrigger != null)
                currentTrigger.SetStartingNode(choice.nextStartingNodeSuccess);

            current = choice.successNode;
        }
        else
        {
            // Recompensas de fallo (stats)
            if (choice.failureRewards != null)
                foreach (var r in choice.failureRewards)
                    StatManager.Instance.IncrementStat(r.statType, r.amount);

            // Recompensas de fallo (items)
            if (choice.failureItemRewards != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddMany(choice.failureItemRewards);
                InventoryUI.RefreshIfAnyOpen();
            }

            choice.onFailure?.Invoke();

            if (choice.nextStartingNodeFailure != null && currentTrigger != null)
                currentTrigger.SetStartingNode(choice.nextStartingNodeFailure);

            current = choice.failureNode;
        }

        Advance();
    }

}