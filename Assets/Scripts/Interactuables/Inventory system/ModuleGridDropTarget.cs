using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
[DisallowMultipleComponent]
public class ModuleGridDropTarget : MonoBehaviour, IDropHandler
{
    [Header("Módulo")]
    public int moduleIndex;

    [Header("Grid con los slots (hijos con ItemSlotUI)")]
    public Transform gridParent;

    // Guarda la última posición de drop en pantalla (la usa ItemSlotDrag)
    public static Vector2 LastDropScreenPos;

    // Cache de los rects de slots para cálculo rápido
    private readonly List<RectTransform> slotRects = new();

    void Awake()
    {
        RebuildSlotCache();
    }

    void OnEnable()
    {
        RebuildSlotCache();
    }

    public void RebuildSlotCache()
    {
        slotRects.Clear();
        if (gridParent == null) return;
        var slots = gridParent.GetComponentsInChildren<ItemSlotUI>(true);
        foreach (var s in slots)
        {
            var rt = s.GetComponent<RectTransform>();
            if (rt != null) slotRects.Add(rt);
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        LastDropScreenPos = eventData.position;
        DragAndDropController.Instance?.NotifyDropped(gameObject);
    }

    public int FindNearestSlotIndex(Vector2 screenPos)
    {
        if (slotRects.Count == 0) RebuildSlotCache();
        if (slotRects.Count == 0) return -1;

        // Intentamos obtener el canvas más cercano (sirve para convertir a screen)
        var canvas = GetComponentInParent<Canvas>();
        Camera cam = canvas ? canvas.worldCamera : null;

        float best = float.MaxValue;
        int bestIdx = -1;

        for (int i = 0; i < slotRects.Count; i++)
        {
            var rt = slotRects[i];
            // centro del rect en pantalla
            Vector2 centerScreen = RectTransformUtility.WorldToScreenPoint(cam, rt.TransformPoint(rt.rect.center));
            float d = Vector2.SqrMagnitude(screenPos - centerScreen);
            if (d < best) { best = d; bestIdx = i; }
        }
        return bestIdx;
    }
}
