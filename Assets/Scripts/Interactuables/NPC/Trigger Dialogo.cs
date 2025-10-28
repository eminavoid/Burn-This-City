using System;
using System.Collections;
using UnityEngine;

public class TriggerDialogo : MonoBehaviour
{
    [Header("Auto start settings")]
    [SerializeField] private bool triggerOnce = true;
    [SerializeField] private float delayBeforeStart = 0f;

    private DialogueTrigger dialogueTrigger;
    private bool hasTriggered;

    private void Awake()
    {
        dialogueTrigger = GetComponent<DialogueTrigger>();
        if (dialogueTrigger == null)
            Debug.LogError($"{name}: DialogueTrigger not found!");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (triggerOnce && hasTriggered) return;

        
        StartCoroutine(StartDialogueAfterDelay());
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        if (delayBeforeStart > 0f)
            yield return new WaitForSeconds(delayBeforeStart);

        if (this != null & this.enabled)
        {
            dialogueTrigger.Interact(null);
            hasTriggered = true;
        }
    }
}
