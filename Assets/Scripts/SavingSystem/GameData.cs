using System;
using System.Collections.Generic;

// 1. El contenedor principal
[Serializable]
public class GameData
{
    public string sceneName; // Para saber qué escena cargar
    public MetaData metaData = new MetaData();
    public PlayerData playerData = new PlayerData();
    public StatsData statsData = new StatsData();
    public InventoryData inventoryData = new InventoryData();
    public DialogueData dialogueData = new DialogueData();
    public List<ContainerData> containerData = new List<ContainerData>();
}

[Serializable]
public class MetaData
{
    // ISO 8601
    public string saveTimestamp;
    public float totalPlaytimeInSeconds;
    public string gameVersion;
}

// 2. Datos del Jugador
[Serializable]
public class PlayerData
{
    public float playerPosX;
    public float playerPosY;
    //SurvivabilityManager
    public float currentHP;
    public float currentSanity;
}

// 3. Datos de Stats (StatManager)
[Serializable]
public class StatsData
{
    public List<StatManager.StatEntry> statEntries = new List<StatManager.StatEntry>();
}

// 4. Datos de Inventario (InventoryManager)
[Serializable]
public class SerializableSlotState
{
    public string itemID; // Usaremos item.name
    public int amount;
}

[Serializable]
public class SerializableModuleState
{
    public List<SerializableSlotState> slots = new List<SerializableSlotState>();
}

[Serializable]
public class InventoryData
{
    public List<SerializableModuleState> modules = new List<SerializableModuleState>();
}

// 5. Datos de Diálogo (NPCs)
[Serializable]
public struct NpcDialogueState
{
    public int npcID;
    public string nodeID;

    public bool hasTalked;
    public bool hasSucceeded;
    public bool hasFailed;
}

    [Serializable]
public class DialogueData
{
    public List<NpcDialogueState> nodeStates = new List<NpcDialogueState>();
}

[Serializable]
public class ContainerData
{
    public string containerID; // El ID único del cofre en la escena
    public List<SerializableSlotState> items = new List<SerializableSlotState>();
}
