using UnityEngine;

public class DestroyOnTalked : MonoBehaviour
{
    // Este método lo va a llamar el UnityEvent
    public void DestroySelf()
    {
        Destroy(gameObject);
    }
}
