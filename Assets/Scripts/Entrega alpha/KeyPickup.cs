using UnityEngine;

public class KeyPickup : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerMovement2D player = other.GetComponent<PlayerMovement2D>();
            if (player != null)
            {
                player.hasKey = true;
                Debug.Log("¡Llave obtenida!");
                Destroy(gameObject);
            }
        }
    }
}
