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

        hasTriggered = true;
        StartCoroutine(StartDialogueAfterDelay());
    }

    private IEnumerator StartDialogueAfterDelay()
    {
        if (delayBeforeStart > 0f)
            yield return new WaitForSeconds(delayBeforeStart);

        // Inicia el diálogo
        dialogueTrigger.Interact(null);

        // Lanza el evento de conversación, si querés que se marque
        dialogueTrigger.RaiseOnTalked();
    }
}
