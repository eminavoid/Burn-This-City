using System;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropController : MonoBehaviour
{
    public static DragAndDropController Instance { get; private set; }

    [Header("Ghost UI")]
    [SerializeField] private Canvas rootCanvas;     // Canvas en modo Screen Space - Overlay
    [SerializeField] private Image ghostImage;      // Image con RaycastTarget desactivado
    [SerializeField] private CanvasGroup ghostGroup;// alpha y blocksRaycasts=false

    private Action<GameObject> onDrop;              // callback cuando droppea sobre algún DropZone
    private bool active;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        HideGhost();
    }

    public void Begin(Sprite icon, Action<GameObject> onDrop)
    {
        this.onDrop = onDrop;

        if (ghostImage != null)
        {
            ghostImage.sprite = icon;
            ghostImage.enabled = true;                 // <- forzar enabled
            ghostImage.raycastTarget = false;          // <- no bloquear drops
            ghostImage.gameObject.SetActive(true);     // <- forzar activo
        }

        if (ghostGroup != null)
        {
            ghostGroup.alpha = 0.95f;
            ghostGroup.blocksRaycasts = false;         // <- no bloquear raycasts
            ghostGroup.interactable = false;
        }

        active = true;
        Debug.Log($"[DnD] Begin: ghost active={ghostImage.gameObject.activeSelf}, enabled={ghostImage.enabled}, sprite={(icon ? icon.name : "NULL")}");
    }


    public void Move(Vector2 screenPos)
    {
        if (!active) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rootCanvas.transform as RectTransform,
            screenPos,
            rootCanvas.worldCamera,
            out var localPoint
        );
        (ghostImage.transform as RectTransform).localPosition = localPoint;
    }

    public void NotifyDropped(GameObject dropTarget)
    {
        if (!active) return;
        onDrop?.Invoke(dropTarget);
        End();
    }

    public void Cancel()
    {
        End();
    }

    private void End()
    {
        active = false;
        onDrop = null;
        HideGhost();
    }

    private void HideGhost()
    {
        if (ghostGroup != null) ghostGroup.alpha = 0f;
        if (ghostImage != null)
        {
            ghostImage.enabled = false;
            ghostImage.sprite = null;
            ghostImage.gameObject.SetActive(false);
        }
    }
}
