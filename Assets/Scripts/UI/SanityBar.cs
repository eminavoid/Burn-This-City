using UnityEngine;
using UnityEngine.UI;

public class SanityBar : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private SurvivabilityManager _stats;
    [SerializeField] private RectTransform barRect;
    [SerializeField] private RectMask2D mask;

    private float maxRightMask;
    private float initialRightMask;

    private void Awake()
    {
        if (_stats == null)
            _stats = SurvivabilityManager.Instance;
    }

    private void Start()
    {
        maxRightMask = 250f;
        initialRightMask = mask.padding.z;
    }

    private void Update()
    {
        if (_stats == null) _stats = SurvivabilityManager.Instance;
        if (_stats == null) return;
        
        float current = _stats.sanityCurrent;
        float max = _stats.sanity.max;

        float normalizedValue = Mathf.Clamp01(current / max);
        UpdateBar(normalizedValue);
    }

    private void UpdateBar(float normalizedValue)
    {
        float targetWidth = normalizedValue * maxRightMask;
        float newRightMask = maxRightMask + initialRightMask - targetWidth;

        var padding = mask.padding;
        padding.z = newRightMask;
        mask.padding = padding;
    }
}
