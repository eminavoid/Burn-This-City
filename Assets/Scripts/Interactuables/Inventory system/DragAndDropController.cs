using System;
using UnityEngine;
using UnityEngine.UI;

public class DragAndDropController : MonoBehaviour
{
    public static DragAndDropController Instance { get; private set; }

    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image ghostImage;
    [SerializeField] private CanvasGroup ghostGroup;

    private Action<GameObject> onDrop;
    private bool active;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this; HideGhost();
    }

    public void Begin(Sprite icon, Action<GameObject> onDrop)
    {
        this.onDrop = onDrop;
        if (ghostImage) { ghostImage.sprite = icon; ghostImage.enabled = (icon != null); ghostImage.raycastTarget = false; ghostImage.gameObject.SetActive(true); }
        if (ghostGroup) { ghostGroup.alpha = 0.95f; ghostGroup.blocksRaycasts = false; }
        active = true;
    }

    public void Move(Vector2 screenPos)
    {
        if (!active || !rootCanvas) return;
        RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)rootCanvas.transform, screenPos, rootCanvas.worldCamera, out var local);
        ((RectTransform)ghostImage.transform).localPosition = local;
    }

    public void NotifyDropped(GameObject dropTarget) { if (!active) return; onDrop?.Invoke(dropTarget); End(); }
    public void Cancel() => End();

    private void End() { active = false; onDrop = null; HideGhost(); }
    private void HideGhost() { if (ghostGroup) ghostGroup.alpha = 0f; if (ghostImage) { ghostImage.enabled = false; ghostImage.sprite = null; ghostImage.gameObject.SetActive(false); } }
}

