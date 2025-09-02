using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField] private InputActionReference interact;
    private StatManager statManager;
    private List<IInteractable> nearbyInteractables = new List<IInteractable>();

    private void Start()
    {
        statManager = StatManager.Instance;
    }
    private void OnEnable()
    {
        interact.action.Enable();
        interact.action.started += OnInteractActionPerformed;
    }

    private void OnDisable()
    {
        interact.action.Disable();
        interact.action.started -= OnInteractActionPerformed;
    }
    // ——— 2D trigger callbacks ———
    private void OnTriggerEnter2D(Collider2D other)
    {
        var interactable = other
            .GetComponents<MonoBehaviour>()
            .OfType<IInteractable>()
            .FirstOrDefault();
        if (interactable == null || nearbyInteractables.Contains(interactable))
            return;

        nearbyInteractables.Add(interactable);
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        var interactable = other
            .GetComponents<MonoBehaviour>()
            .OfType<IInteractable>()
            .FirstOrDefault();

        nearbyInteractables.Remove(interactable);
    }
    private void OnInteractActionPerformed(InputAction.CallbackContext context)
    {
        InteractWithNearest();
    }
    private void InteractWithNearest()
    {
        var target = nearbyInteractables.FirstOrDefault();
        if (target == null)
        {
            return;
        }
        if (statManager == null)
        {
            return;
        }
        if (!target.CanInteract(statManager))
        {
            return;
        }
        target.Interact(statManager);
    }
}