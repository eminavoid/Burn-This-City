using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInteraction : MonoBehaviour
{
    [SerializeField]
    private InputActionReference Intereact;
    StatManager statManager = StatManager.Instance;
    private List<Interactable> nearbyInteractables = new List<Interactable>();


    private void OnEnable()
    {
        Intereact.action.Enable();
        Intereact.action.performed += OnInteractActionPerformed;
    }

    private void OnDisable()
    {   
        Intereact.action.Disable();
        Intereact.action.performed -= OnInteractActionPerformed;
    }

    private void OnTriggerEnter(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            if (!nearbyInteractables.Contains(interactable))
            {
                nearbyInteractables.Add(interactable);
            }
            Debug.Log("ITEM");
        }
    }
    private void OnTriggerExit(Collider other)
    {
        Interactable interactable = other.GetComponent<Interactable>();
        if (interactable != null)
        {
            nearbyInteractables.Remove(interactable);
        }
    }
    private void OnInteractActionPerformed(InputAction.CallbackContext context)
    {
        InteractWithNearest();
    }
    private void InteractWithNearest()
    {
        IInteractable targetInteractable = nearbyInteractables.FirstOrDefault();
        if (statManager != null)
        {
            if (targetInteractable.CanInteract(statManager))
            {
                targetInteractable.Interact(statManager);
            }
            else
            {
                Debug.Log("No tienes los requisitos para interactuar con este objeto.");
                // Optionally, show a message to the player about the missing requirements.
            }
        }
        else
        {
            Debug.LogError("No se encontró la instancia del StatManager.");
        }

    }

}
