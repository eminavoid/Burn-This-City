using UnityEngine;

public class SampleInteractableDoor : Interactable
{
    public string itemName = "Objeto Ejemplo";

    public override void Interact(StatManager statManager)
    {
        if (CanInteract(statManager))
        {
            Debug.Log($"Has interactuado con el objeto: {itemName}.");
        }
        else
        {
            Debug.Log($"No tienes los requisitos para interactuar con {itemName}.");
        }
    }


}