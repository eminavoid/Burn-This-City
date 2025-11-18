using UnityEngine;
using System.Collections.Generic;

public class StatManager : MonoBehaviour
{
    [SerializeField] int statsStartingValue = 1;

    public static StatManager Instance { get; private set; }

    public static event System.Action<StatType, int> OnStatChanged;

    public enum StatType
    {
        Knowledge,
        Logic,
        Perception,
        Dexterity,
        Robustness,
        Vigor,
        Coaxing,
        Intimidation,
        Trickery
    }
    [System.Serializable]
    public struct StatEntry
    {
        public StatType statType;
        public int value;
    }

    [SerializeField]
    private List<StatEntry> statEntries = new List<StatEntry>();
    private Dictionary<StatType, int> stats = new Dictionary<StatType, int>();
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);

            if (statEntries.Count == 0)
            {
                foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
                {
                    statEntries.Add(new StatEntry { statType = stat, value = statsStartingValue });
                }
            }

            SyncDictFromList();
            foreach (var entry in statEntries)
            {
                OnStatChanged?.Invoke(entry.statType, entry.value);
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }
#if UNITY_EDITOR
    private void OnValidate()
    {
        SyncDictFromList();
    }
#endif
    private void SyncDictFromList()
    {
        stats.Clear();
        foreach (var entry in statEntries)
        {
            stats[entry.statType] = entry.value;
        }
    }
    private void UpdateOrAddEntry(StatType type, int value)
    {
        for (int i = 0; i < statEntries.Count; i++)
        {
            if (statEntries[i].statType == type)
            {
                statEntries[i] = new StatEntry { statType = type, value = value };
                return;
            }
        }
        statEntries.Add(new StatEntry { statType = type, value = value });
    }
    public void SetStat(StatType type, int value)
    {
        stats[type] = value;
        UpdateOrAddEntry(type, value);

        OnStatChanged?.Invoke(type, value);
    }
    public int GetStat(StatType type)
    {
        if (stats.TryGetValue(type, out int value))
            return value;

        Debug.LogWarning($"Stat '{type}' not found, returning 0.");
        return 0;
    }
    public void IncrementStat(StatType type, int amount)
    {
        int newValue = Mathf.Max(0, GetStat(type) + amount);
        SetStat(type, newValue);             
    }
    public void ResetAllStats()
    {
        foreach (StatType stat in System.Enum.GetValues(typeof(StatType)))
        {
            SetStat(stat, statsStartingValue);
        }

        Debug.Log("Todas las estadísticas han sido reseteadas a 3.");
    }

    public List<StatEntry> GetStatEntries()
    {
        return statEntries;
    }

    public void LoadStats(List<StatEntry> loadedEntries)
    {
        if (loadedEntries == null) return;

        statEntries = new List<StatEntry>(loadedEntries);
        SyncDictFromList();     

        foreach (var entry in statEntries)
        {
            OnStatChanged?.Invoke(entry.statType, entry.value);
        }
        Debug.Log("Stats cargados en StatManager.");
    }
}