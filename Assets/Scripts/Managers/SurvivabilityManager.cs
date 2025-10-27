using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public enum SurvivabilityStat { HP, Food, Water, Sanity }
public enum StatusType { Increase, Decrease }

[Serializable] public class StatChangedEvent : UnityEvent<SurvivabilityStat, float, float> { }
[Serializable] public class StatZeroEvent : UnityEvent<SurvivabilityStat> { }
[Serializable] public class DiedEvent : UnityEvent { }
[Serializable] public class TickingChangedEvent : UnityEvent<bool> { } // true = ticking

[Serializable]
public struct StatSetup
{
    [Min(0)] public float max;
    [Min(0)] public float start;
    [Tooltip("How much this stat decays on each tick (before modifiers).")]
    [Min(0)] public float baseDecayPerTick;
    public float Clamp(float v) => Mathf.Clamp(v, 0f, max);
}

public sealed class SurvivabilityManager : MonoBehaviour
{
    // -------------------- Singleton --------------------
    public static SurvivabilityManager Instance { get; private set; }

    [Header("Singleton")]
    [Tooltip("Keep this manager across scene loads.")]
    public bool dontDestroyOnLoad = true;

    private void EnsureSingleton()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        if (dontDestroyOnLoad) DontDestroyOnLoad(gameObject);
    }
    // ---------------------------------------------------

    [Header("Tick")]
    [Tooltip("Seconds between ticks. Every tick applies decay to each stat.")]
    [Min(0.05f)] public float tickRateSeconds = 1f;

    [Tooltip("Use unscaled time for ticking (ignores Time.timeScale).")]
    public bool useUnscaledTime = false;

    [Tooltip("If enabled, ticking is auto-paused whenever Time.timeScale == 0.")]
    public bool autoPauseWhenTimescaleZero = true;

    [Header("Stats")]
    public StatSetup hp = new StatSetup { max = 100, start = 100, baseDecayPerTick = 0.1f };
    public StatSetup food = new StatSetup { max = 100, start = 100, baseDecayPerTick = 0.5f };
    public StatSetup water = new StatSetup { max = 100, start = 100, baseDecayPerTick = 0.8f };
    public StatSetup sanity = new StatSetup { max = 100, start = 100, baseDecayPerTick = 0.2f };

    [Header("HP Rules")]
    [Tooltip("Optionally apply extra HP decay when other survivability stats are empty.")]
    public bool hpDecaysWhenOtherStatsEmpty = true;
    [Tooltip("Extra HP decay per empty stat (Food/Water/Sanity) each tick.")]
    [Min(0)] public float hpExtraDecayPerEmptyStat = 1f;

    [Header("Events")]
    public StatChangedEvent OnStatChanged;
    public StatZeroEvent OnStatZero;
    public DiedEvent OnDied;
    public TickingChangedEvent OnTickingChanged;

    // -------- Inspector-exposed runtime values --------
    [Header("Runtime (Current Values)")]
    [Tooltip("If enabled, editing these during Play Mode will overwrite internal values (clamped).")]
    public bool syncFromInspectorInPlayMode = false;

    [SerializeField, Min(0)] private float hpCurrent = 100f;
    [SerializeField, Min(0)] private float foodCurrent = 100f;
    [SerializeField, Min(0)] private float waterCurrent = 100f;
    [SerializeField, Min(0)] private float sanityCurrent = 100f;

    [Space(4)]
    [SerializeField] private bool isTicking; // read-only mirror
    [SerializeField] private List<string> pauseReasons = new(); // read-only mirror

    // Internal state
    private readonly Dictionary<SurvivabilityStat, float> _current = new();
    private readonly Dictionary<SurvivabilityStat, float> _max = new();
    private readonly Dictionary<SurvivabilityStat, float> _baseDecay = new();

    // Decay multipliers (stack multiplicatively). 1.0 = no change.
    private readonly Dictionary<SurvivabilityStat, float> _decayMultiplier = new()
    {
        { SurvivabilityStat.HP, 1f },
        { SurvivabilityStat.Food, 1f },
        { SurvivabilityStat.Water, 1f },
        { SurvivabilityStat.Sanity, 1f },
    };

    private readonly List<ActiveEffect> _effects = new();

    // Ticking control
    private bool _tickingEnabled = true;      // explicit switch via SetTicking
    private readonly HashSet<string> _pauses = new(); // named pause tokens
    private float _accum;

    // -------------------- Unity --------------------
    private void Awake()
    {
        EnsureSingleton();
        if (Instance != this) return;

        InitStat(SurvivabilityStat.HP, hp);
        InitStat(SurvivabilityStat.Food, food);
        InitStat(SurvivabilityStat.Water, water);
        InitStat(SurvivabilityStat.Sanity, sanity);

        MirrorAllToInspector();
        RecomputeTickingState(raiseEvent: false);
    }

    private void Update()
    {
        if (syncFromInspectorInPlayMode) SyncFromInspector();

        // Re-evaluate auto pause (e.g., timeScale changes)
        RecomputeTickingState();

        if (!isTicking) return;

        float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
        _accum += dt;

        // update timed effects
        for (int i = _effects.Count - 1; i >= 0; --i)
        {
            var e = _effects[i];
            e.timeLeft -= dt;
            if (e.timeLeft <= 0f)
            {
                RemoveEffectContribution(e);
                _effects.RemoveAt(i);
            }
            else
            {
                _effects[i] = e;
            }
        }

        if (_accum >= Mathf.Max(0.01f, tickRateSeconds))
        {
            int ticks = Mathf.FloorToInt(_accum / tickRateSeconds);
            _accum -= ticks * tickRateSeconds;
            for (int i = 0; i < ticks; i++) TickOnce();
        }
    }
    // ----------------------------------------------

    private void InitStat(SurvivabilityStat s, StatSetup setup)
    {
        _max[s] = Mathf.Max(1f, setup.max);
        _current[s] = Mathf.Clamp(setup.start, 0f, _max[s]);
        _baseDecay[s] = Mathf.Max(0f, setup.baseDecayPerTick);
        RaiseChanged(s);
    }

    private void TickOnce()
    {
        ApplyDecay(SurvivabilityStat.Food);
        ApplyDecay(SurvivabilityStat.Water);
        ApplyDecay(SurvivabilityStat.Sanity);
        ApplyDecay(SurvivabilityStat.HP);

        if (hpDecaysWhenOtherStatsEmpty)
        {
            int empties = (IsZero(SurvivabilityStat.Food) ? 1 : 0)
                        + (IsZero(SurvivabilityStat.Water) ? 1 : 0)
                        + (IsZero(SurvivabilityStat.Sanity) ? 1 : 0);
            if (empties > 0 && hpExtraDecayPerEmptyStat > 0f)
            {
                Decrease(SurvivabilityStat.HP, hpExtraDecayPerEmptyStat * empties, silent: false);
            }
        }

        if (IsZero(SurvivabilityStat.HP))
        {
            OnDied?.Invoke();
        }
    }

    private void ApplyDecay(SurvivabilityStat s)
    {
        float decay = _baseDecay[s] * Mathf.Max(0f, _decayMultiplier[s]);
        if (decay <= 0f) return;
        Decrease(s, decay, silent: false);
    }

    // ---------- Public API: Ticking control ----------

    /// <summary> Read-only: is the system currently ticking? </summary>
    public bool IsTicking => isTicking;

    /// <summary> Enable/disable ticking explicitly. </summary>
    public void SetTicking(bool enabled)
    {
        _tickingEnabled = enabled;
        RecomputeTickingState();
    }

    /// <summary> Pause ticking with a named reason (e.g., "Menu"). Safe to call multiple times; uses a set. </summary>
    public void PushPause(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) reason = "Unnamed";
        _pauses.Add(reason);
        RecomputeTickingState();
    }

    /// <summary> Remove a previously added pause reason. If no reasons remain, ticking may resume. </summary>
    public void PopPause(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason)) reason = "Unnamed";
        _pauses.Remove(reason);
        RecomputeTickingState();
    }

    private void RecomputeTickingState(bool raiseEvent = true)
    {
        bool autoPaused = autoPauseWhenTimescaleZero && (Time.timeScale <= 0f);
        bool newState = _tickingEnabled && !autoPaused && _pauses.Count == 0;

        if (newState != isTicking)
        {
            isTicking = newState;
            // mirror pause reasons to inspector list
            pauseReasons.Clear();
            foreach (var r in _pauses) pauseReasons.Add(r);

            if (raiseEvent) OnTickingChanged?.Invoke(isTicking);
        }
        else
        {
            // keep inspector mirrors fresh
            pauseReasons.Clear();
            foreach (var r in _pauses) pauseReasons.Add(r);
        }
    }

    // ---------- Public API: Stats ----------

    public float Get(SurvivabilityStat s) => _current[s];
    public float GetMax(SurvivabilityStat s) => _max[s];
    public float Get01(SurvivabilityStat s) => _max[s] <= 0.0001f ? 0f : _current[s] / _max[s];

    public void SetTickRate(float seconds) => tickRateSeconds = Mathf.Max(0.01f, seconds);

    public void Increase(SurvivabilityStat s, float amount, bool silent = false)
    {
        if (amount <= 0f) return;
        _current[s] = Mathf.Min(_current[s] + amount, _max[s]);
        if (!silent) RaiseChanged(s);
    }

    public void Decrease(SurvivabilityStat s, float amount, bool silent = false)
    {
        if (amount <= 0f) return;
        float before = _current[s];
        _current[s] = Mathf.Max(0f, _current[s] - amount);
        if (!silent)
        {
            RaiseChanged(s);
            if (before > 0f && Mathf.Approximately(_current[s], 0f)) OnStatZero?.Invoke(s);
        }
    }

    public void SetMax(SurvivabilityStat s, float newMax, bool keepPercent = true)
    {
        newMax = Mathf.Max(1f, newMax);
        float pct = keepPercent ? Get01(s) : Mathf.Min(Get(s), newMax) / newMax;
        _max[s] = newMax;
        _current[s] = Mathf.Clamp01(pct) * newMax;
        RaiseChanged(s);
    }

    public void SetBaseDecay(SurvivabilityStat s, float newDecayPerTick)
    {
        _baseDecay[s] = Mathf.Max(0f, newDecayPerTick);
    }

    /// <summary> multiplier 0.5f halves decay; 2f doubles decay; 0f pauses decay. </summary>
    public void ApplyDecayModifier(SurvivabilityStat s, float multiplier, float durationSeconds, StatusType tag = StatusType.Increase)
    {
        var effect = new ActiveEffect
        {
            stat = s,
            multiplier = Mathf.Max(0f, multiplier),
            timeLeft = Mathf.Max(0.01f, durationSeconds),
            type = tag
        };
        _effects.Add(effect);
        _decayMultiplier[s] *= effect.multiplier;
    }

    /// <summary> Positive heals/restores over duration; negative drains over duration. </summary>
    public void ApplyOverTime(SurvivabilityStat s, float totalAmount, float durationSeconds)
    {
        if (Mathf.Approximately(totalAmount, 0f) || durationSeconds <= 0f) return;
        StartCoroutine(OverTimeRoutine(s, totalAmount, durationSeconds));
    }

    // ---------- Helpers ----------
    private System.Collections.IEnumerator OverTimeRoutine(SurvivabilityStat s, float total, float duration)
    {
        float elapsed = 0f;
        float start = Get(s);
        while (elapsed < duration)
        {
            float dt = useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            elapsed += dt;
            float t = Mathf.Clamp01(elapsed / duration);
            float target = start + total * t;
            float delta = target - Get(s);
            if (delta > 0f) Increase(s, delta, silent: false);
            else Decrease(s, -delta, silent: false);
            yield return null;
        }
    }

    private bool IsZero(SurvivabilityStat s) => _current[s] <= 0.0001f;

    private void RaiseChanged(SurvivabilityStat s)
    {
        MirrorOneToInspector(s);
        OnStatChanged?.Invoke(s, _current[s], _max[s]);
    }

    private struct ActiveEffect
    {
        public SurvivabilityStat stat;
        public float multiplier;
        public float timeLeft;
        public StatusType type;
    }

    private void RemoveEffectContribution(ActiveEffect e)
    {
        if (e.multiplier > 0f)
        {
            _decayMultiplier[e.stat] /= e.multiplier;
            if (!float.IsFinite(_decayMultiplier[e.stat]) || _decayMultiplier[e.stat] <= 0f)
                _decayMultiplier[e.stat] = 1f;
        }
    }

    // -------- Inspector mirror logic --------
    private void MirrorAllToInspector()
    {
        hpCurrent = _current[SurvivabilityStat.HP];
        foodCurrent = _current[SurvivabilityStat.Food];
        waterCurrent = _current[SurvivabilityStat.Water];
        sanityCurrent = _current[SurvivabilityStat.Sanity];
    }

    private void MirrorOneToInspector(SurvivabilityStat s)
    {
        switch (s)
        {
            case SurvivabilityStat.HP: hpCurrent = _current[s]; break;
            case SurvivabilityStat.Food: foodCurrent = _current[s]; break;
            case SurvivabilityStat.Water: waterCurrent = _current[s]; break;
            case SurvivabilityStat.Sanity: sanityCurrent = _current[s]; break;
        }
    }

    private void SyncFromInspector()
    {
        if (!Application.isPlaying) return;
        SetCurrentFromInspector(SurvivabilityStat.HP, ref hpCurrent, hp.max);
        SetCurrentFromInspector(SurvivabilityStat.Food, ref foodCurrent, food.max);
        SetCurrentFromInspector(SurvivabilityStat.Water, ref waterCurrent, water.max);
        SetCurrentFromInspector(SurvivabilityStat.Sanity, ref sanityCurrent, sanity.max);
    }

    private void SetCurrentFromInspector(SurvivabilityStat s, ref float field, float max)
    {
        float clamped = Mathf.Clamp(field, 0f, Mathf.Max(1f, max));
        if (!Mathf.Approximately(clamped, _current[s]))
        {
            _current[s] = clamped;
            field = clamped;
            OnStatChanged?.Invoke(s, _current[s], _max[s]);
        }
    }
}
