using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SurvivabilityStat { HP, Sanity }

[Serializable] public class StatChangedEvent : UnityEvent<SurvivabilityStat, float, float> { }
[Serializable] public class StatZeroEvent : UnityEvent<SurvivabilityStat> { }
[Serializable] public class DiedEvent : UnityEvent { }

[Serializable]
public struct StatSetup
{
    [Min(0)] public float max;
    [Min(0)] public float start;
    public float Clamp(float v) => Mathf.Clamp(v, 0f, max);
}

public sealed class SurvivabilityManager : MonoBehaviour
{
    public static SurvivabilityManager Instance { get; private set; }

    [Header("Singleton")]
    public bool dontDestroyOnLoad = true;

    [Header("Stats")]
    public StatSetup hp = new StatSetup { max = 100, start = 100 };
    public StatSetup sanity = new StatSetup { max = 100, start = 100 };
    
    public bool IsInitialized { get; private set; } = false;

    [Header("Events")]
    public StatChangedEvent OnStatChanged;
    public StatZeroEvent OnStatZero;
    public DiedEvent OnDied;

    [SerializeField, Min(0)] public float hpCurrent = 100f;
    [SerializeField, Min(0)] public float sanityCurrent = 100f;

    private readonly Dictionary<SurvivabilityStat, float> _current = new();
    private readonly Dictionary<SurvivabilityStat, float> _max = new();
    

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);

        InitStat(SurvivabilityStat.HP, hp);
        InitStat(SurvivabilityStat.Sanity, sanity);

        IsInitialized = true; // 👈 marcamos que ya está listo
    }

    private void InitStat(SurvivabilityStat s, StatSetup setup)
    {
        _max[s] = Mathf.Max(1f, setup.max);
        _current[s] = Mathf.Clamp(setup.start, 0f, _max[s]);
        RaiseChanged(s);
    }

    // ===========================
    // Public API
    // ===========================

    public float Get(SurvivabilityStat s) => _current[s];
    public float GetMax(SurvivabilityStat s) => _max[s];
    public float Get01(SurvivabilityStat s) => _max[s] <= 0.0001f ? 0f : _current[s] / _max[s];

    public void ModifyHealth(float amount)
    {
        if (amount == 0f) return;
        if (amount > 0f)
            Increase(SurvivabilityStat.HP, amount);
        else
            Decrease(SurvivabilityStat.HP, -amount);
    }

    public void ModifySanity(float amount)
    {
        if (amount == 0f) return;
        if (amount > 0f)
            Increase(SurvivabilityStat.Sanity, amount);
        else
            Decrease(SurvivabilityStat.Sanity, -amount);
    }

    public void Increase(SurvivabilityStat s, float amount)
    {
        if (amount <= 0f) return;
        _current[s] = Mathf.Min(_current[s] + amount, _max[s]);
        RaiseChanged(s);
    }

    public void Decrease(SurvivabilityStat s, float amount)
    {
        if (amount <= 0f) return;
        float before = _current[s];
        _current[s] = Mathf.Max(0f, _current[s] - amount);
        RaiseChanged(s);

        if (before > 0f && Mathf.Approximately(_current[s], 0f))
        {
            OnStatZero?.Invoke(s);
            if (s == SurvivabilityStat.HP)
                OnDied?.Invoke();
        }
    }

    private void RaiseChanged(SurvivabilityStat s)
    {
        OnStatChanged?.Invoke(s, _current[s], _max[s]);
        if (s == SurvivabilityStat.HP) hpCurrent = _current[s];
        if (s == SurvivabilityStat.Sanity) sanityCurrent = _current[s];
    }
}
