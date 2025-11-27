using UnityEngine;
using System.Linq;
using System;

public class DialogueRunner : MonoBehaviour
{
    public static DialogueRunner Instance { get; private set; }

    [SerializeField] private DialogueUI ui;
    [SerializeField] private AudioClip successSFX;
    [SerializeField] private AudioClip failureSFX;
    [SerializeField] private AudioClip nextDialogueSFX;
    [SerializeField] private AudioClip endDialogueSFX;

    public DialogueNode current;
    private DialogueTrigger currentTrigger;

    public static event Action DialogueStarted;
    public static event Action DialogueEnded;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public void Begin(DialogueNode start, DialogueTrigger trigger)
    {
        currentTrigger = trigger;

        if (currentTrigger != null)
        {
            currentTrigger.OnSucces.RemoveListener(HandleOnSucces);
            currentTrigger.OnFailure.RemoveListener(HandleOnFailure);

            currentTrigger.OnSucces.AddListener(HandleOnSucces);
            currentTrigger.OnFailure.AddListener(HandleOnFailure);
        }

        DialogueStarted?.Invoke();
        current = start;
        Advance();
    }

    private void HandleOnSucces()
    {
        if (AudioManager.Instance != null && successSFX != null)
            AudioManager.Instance.PlaySFX(successSFX);
    }

    private void HandleOnFailure()
    {
        if (AudioManager.Instance != null && failureSFX != null)
            AudioManager.Instance.PlaySFX(failureSFX);
    }

    private void Advance()
    {
        if (current == null || current.choices == null)
        {
            if (AudioManager.Instance != null && endDialogueSFX != null)
                AudioManager.Instance.PlaySFX(endDialogueSFX);

            ui.Hide();
            DialogueEnded?.Invoke();

            if (AutoSaver.Instance != null)
            {
                Debug.Log("[DialogueRunner] Conversación finalizada. Guardando...");
                AutoSaver.Instance.TriggerAutoSave();
            }

            return;
        }

        if (AudioManager.Instance != null && nextDialogueSFX != null)
            AudioManager.Instance.PlaySFX(nextDialogueSFX, 0.8f);

        var options = current.choices
                             .Select((c, i) => (c.playerText, i))
                             .ToList();
        ui.Show(current.npcName, current.npcText, options);
    }

    // ?? NUEVO: Maneja tanto Stats como Health y Sanity
    private void ApplyReward(DialogueReward reward)
    {
        switch (reward.rewardType)
        {
            case DialogueReward.RewardType.Stat:
                StatManager.Instance.IncrementStat(reward.statType, reward.amount);
                break;
            case DialogueReward.RewardType.Health:
                SurvivabilityManager.Instance?.ModifyHealth(reward.amount);
                break;
            case DialogueReward.RewardType.Sanity:
                SurvivabilityManager.Instance?.ModifySanity(reward.amount);
                break;
        }
    }

    public void Choose(int index)
    {
        var choice = current.choices[index];

        bool hasStatReqs = choice.statRequirements != null && choice.statRequirements.Count > 0;
        bool hasItemReqs = choice.itemRequirements != null && choice.itemRequirements.Count > 0;

        // ——— Caso sin requisitos ———
        if (!hasStatReqs && !hasItemReqs)
        {
            if (choice.grantedReward != null)
                foreach (var r in choice.grantedReward)
                    ApplyReward(r);

            if (choice.defaultItemRewards != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddMany(choice.defaultItemRewards);
                InventoryUI.RefreshIfAnyOpen();
            }

            if (choice.raiseOnTalked)
                currentTrigger?.RaiseOnTalked();

            if (choice.nextStartingNodeDefault != null && currentTrigger != null)
                currentTrigger.SetStartingNode(choice.nextStartingNodeDefault);

            current = choice.defaultNode;
            Advance();
            return;
        }

        // ——— Caso con requisitos ———
        bool statsOk = true;
        if (hasStatReqs)
            statsOk = choice.statRequirements.All(req => req.IsMet(StatManager.Instance.GetStat(req.statType)));

        bool itemsOk = true;
        if (hasItemReqs)
            itemsOk = InventoryManager.Instance != null && InventoryManager.Instance.CanConsume(choice.itemRequirements);

        bool success = statsOk && itemsOk;

        if (success)
        {
            if (hasItemReqs && choice.consumeRequirementsOnSuccess && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.Consume(choice.itemRequirements);
                InventoryUI.RefreshIfAnyOpen();
            }

            if (choice.successRewards != null)
                foreach (var r in choice.successRewards)
                    ApplyReward(r);

            if (choice.successItemRewards != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddMany(choice.successItemRewards);
                InventoryUI.RefreshIfAnyOpen();
            }

            if (choice.raiseOnSuccess)
                currentTrigger?.RaiseOnSucces();

            if (choice.nextStartingNodeSuccess != null && currentTrigger != null)
                currentTrigger.SetStartingNode(choice.nextStartingNodeSuccess);

            current = choice.successNode;
        }
        else
        {
            if (choice.failureRewards != null)
                foreach (var r in choice.failureRewards)
                    ApplyReward(r);

            if (choice.failureItemRewards != null && InventoryManager.Instance != null)
            {
                InventoryManager.Instance.AddMany(choice.failureItemRewards);
                InventoryUI.RefreshIfAnyOpen();
            }

            if (choice.raiseOnFailure)
                currentTrigger?.RaiseOnFailure();

            if (choice.nextStartingNodeFailure != null && currentTrigger != null)
                currentTrigger.SetStartingNode(choice.nextStartingNodeFailure);

            current = choice.failureNode;
        }

        Advance();
    }
}
