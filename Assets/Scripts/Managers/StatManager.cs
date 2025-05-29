using UnityEngine;
using System.Collections.Generic;

public class StatManager : MonoBehaviour
{
    public static StatManager Instance { get; private set; }

    public Dictionary<StatType, int> stats = new Dictionary<StatType, int>();
    public enum StatType
    {
        Strength,
        Dexterity,
        Constitution,
        Intelligence,
        Wisdom,
        Charisma
    }
    private void Awake()
    {
        // Implementación del Singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Evita que este GameObject se destruya al cargar nuevas escenas
        }
        else
        {
            Destroy(gameObject); // Si ya existe otra instancia, destrúyela
        }
    }
    public void SetStat(StatType type, int value)
    {
        if (stats.ContainsKey(type))
        {
            stats[type] = value;
        }
        else
        {
            stats.Add(type, value);
        }
    }
    public int GetStat(StatType type)
    {
        return stats.ContainsKey(type) ? stats[type] : 0;
    }
}