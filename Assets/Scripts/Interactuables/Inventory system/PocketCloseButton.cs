using UnityEngine;

public class PocketCloseButton : MonoBehaviour
{
    [SerializeField, Min(0)] private int pocketIndex;

    public void ClosePocket()
    {
        if (InventoryUI.Instance != null)
            InventoryUI.Instance.ClosePocket(pocketIndex);
    }
}
