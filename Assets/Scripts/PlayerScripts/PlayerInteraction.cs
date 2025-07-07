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
        Debug.Log("Trigger entered: " + other.name); 
        var interactable = other
            .GetComponents<MonoBehaviour>()
            .OfType<IInteractable>()
            .FirstOrDefault();
        if (interactable == null || nearbyInteractables.Contains(interactable))
            return;

        nearbyInteractables.Add(interactable);
        Debug.Log("addITEM");
    }
    private void OnTriggerExit2D(Collider2D other)
    {
        Debug.Log("Trigger exited: " + other.name);
        var interactable = other
            .GetComponents<MonoBehaviour>()
            .OfType<IInteractable>()
            .FirstOrDefault();

        if (interactable != null && nearbyInteractables.Remove(interactable))
            Debug.Log("removeITEM");
    }
    private void OnInteractActionPerformed(InputAction.CallbackContext context)
    {
        InteractWithNearest();
        Debug.Log("Interact action performed.");
    }
    private void InteractWithNearest()
    {
        var target = nearbyInteractables.FirstOrDefault();
        if (target == null)
        {
            Debug.Log("No hay ningún objeto cercano con el que interactuar.");
            return;
        }
        if (statManager == null)
        {
            Debug.LogError("No se encontró la instancia de StatManager.");
            return;
        }
        if (!target.CanInteract(statManager))
        {
            Debug.Log("No tienes los requisitos para interactuar con este objeto.");
            return;
        }
        target.Interact(statManager);
    }
}