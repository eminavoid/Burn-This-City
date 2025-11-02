using UnityEngine;
using System.Collections.Generic;
using System.IO;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private string saveFilePath;

    private Dictionary<int, DialogueNode> sceneDialogueState = new Dictionary<int, DialogueNode>();

    private GameData dataToLoad = null;

    private float totalPlaytimeInSeconds = 0f;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        saveFilePath = Path.Combine(Application.persistentDataPath, "burnthiscity.json");

        SceneManager.sceneLoaded += OnSceneLoaded;
    }
    private void Update()
    {
        totalPlaytimeInSeconds += Time.deltaTime;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // API de Persistencia de Escena (Modo 1)
    public void UpdateNpcNode(int npcID, DialogueNode node)
    {
        if (node != null)
        {
            sceneDialogueState[npcID] = node;
        }
    }

    // DialogueTrigger Start()
    public DialogueNode GetNpcNode(int npcID)
    {
        if (sceneDialogueState.TryGetValue(npcID, out DialogueNode node))
        {
            return node;
        }
        return null;
    }

    // --- API de Guardado/Cargado de Juego (Modo 2) ---

    public void SaveGame()
    {
        Debug.Log("Guardando partida...");
        GameData gameData = new GameData();

        // 0. GUARDAR METADATA
        gameData.metaData.saveTimestamp = System.DateTime.Now.ToString("o"); // "o" es formato ISO 8601
        gameData.metaData.totalPlaytimeInSeconds = this.totalPlaytimeInSeconds;
        gameData.metaData.gameVersion = Application.version;

        // 1. Guardar Escena y Posición del Jugador
        gameData.sceneName = SceneManager.GetActiveScene().name;
        PlayerMovement2D player = FindFirstObjectByType<PlayerMovement2D>();
        if (player != null)
        {
            gameData.playerData.playerPosX = player.transform.position.x;
            gameData.playerData.playerPosY = player.transform.position.y;
        }

        // 2. Guardar Survivability
        gameData.playerData.currentHP = SurvivabilityManager.Instance.Get(SurvivabilityStat.HP);
        gameData.playerData.currentSanity = SurvivabilityManager.Instance.Get(SurvivabilityStat.Sanity);

        // 3. Guardar Stats
        gameData.statsData.statEntries = StatManager.Instance.GetStatEntries();

        // 4. Guardar Inventario (Traducción Item -> ItemID)
        gameData.inventoryData.modules = new List<SerializableModuleState>();
        foreach (var module in InventoryManager.Instance.Modules)
        {
            var newSavedModule = new SerializableModuleState();
            foreach (var slot in module.slots)
            {
                var newSavedSlot = new SerializableSlotState();
                if (slot.item != null)
                {
                    // item.name como ID unico.
                    // items en Resources/Items/
                    newSavedSlot.itemID = slot.item.name;
                    newSavedSlot.amount = slot.amount;
                }
                newSavedModule.slots.Add(newSavedSlot);
            }
            gameData.inventoryData.modules.Add(newSavedModule);
        }

        // 5. Guardar Estado de Diálogos
        gameData.dialogueData.nodeStates = new List<NpcDialogueState>();
        foreach (var kvp in sceneDialogueState)
        {
            // node.name como ID unico
            // El nodo DEBE estar en una carpeta "Resources/DialogueNodes/"
            gameData.dialogueData.nodeStates.Add(new NpcDialogueState
            {
                npcID = kvp.Key,
                nodeID = (kvp.Value != null) ? kvp.Value.name : null
            });
        }

        // Serializar y Escribir en Disco
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(saveFilePath, json);
        Debug.Log($"Partida guardada en: {saveFilePath}");
    }

    public void LoadGame()
    {
        if (!File.Exists(saveFilePath))
        {
            Debug.LogWarning("No se encontró archivo de guardado.");
            return;
        }

        Debug.Log("Cargando partida...");
        string json = File.ReadAllText(saveFilePath);
        GameData gameData = JsonUtility.FromJson<GameData>(json);

        // Guardamos los datos temporalmente y cargamos la escena.
        // OnSceneLoaded se encargará de aplicar los datos.
        this.dataToLoad = gameData;
        SceneManager.LoadScene(gameData.sceneName);
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (this.dataToLoad == null)
        {
            return;
        }

        Debug.Log("Aplicando datos cargados a la escena...");

        PlayerMovement2D player = FindFirstObjectByType<PlayerMovement2D>();
        if (player != null)
        {
            player.transform.position = new Vector2(dataToLoad.playerData.playerPosX, dataToLoad.playerData.playerPosY);
        }
        SurvivabilityManager.Instance.SetHealth(dataToLoad.playerData.currentHP);
        SurvivabilityManager.Instance.SetSanity(dataToLoad.playerData.currentSanity);


        // 2. Aplicar Stats
        StatManager.Instance.LoadStats(dataToLoad.statsData.statEntries);

        // 3. Aplicar Inventario (Traducción ItemID -> Item)
        var inv = InventoryManager.Instance;
        inv.InitModules(); // Resetea el inventario a slots vacíos

        var loadedModules = dataToLoad.inventoryData.modules;
        for (int m = 0; m < inv.Modules.Count; m++)
        {
            if (m >= loadedModules.Count) break;
            var moduleToLoad = loadedModules[m];
            var targetModule = inv.Modules[m];

            for (int s = 0; s < targetModule.slots.Count; s++)
            {
                if (s >= moduleToLoad.slots.Count) break;
                var slotToLoad = moduleToLoad.slots[s];
                var targetSlot = targetModule.slots[s];

                if (!string.IsNullOrEmpty(slotToLoad.itemID))
                {
                    targetSlot.item = Resources.Load<InventoryItem>("Items/" + slotToLoad.itemID);
                    targetSlot.amount = slotToLoad.amount;
                    if (targetSlot.item == null)
                    {
                        Debug.LogWarning($"No se pudo cargar el item con ID: {slotToLoad.itemID}");
                        targetSlot.amount = 0;
                    }
                }
            }
        }
        inv.ForceRefresh();

        // 4. Aplicar Estado de Diálogos
        sceneDialogueState.Clear();
        foreach (var npcState in dataToLoad.dialogueData.nodeStates)
        {
            if (!string.IsNullOrEmpty(npcState.nodeID))
            {
                // ASUNCIÓN CRÍTICA: Carga el nodo desde "Resources/DialogueNodes/"
                DialogueNode node = Resources.Load<DialogueNode>("DialogueNodes/" + npcState.nodeID);
                if (node != null)
                {
                    sceneDialogueState[npcState.npcID] = node;
                }
                else
                {
                    Debug.LogWarning($"No se pudo cargar el nodo con ID: {npcState.nodeID}");
                }
            }
        }

        this.totalPlaytimeInSeconds = dataToLoad.metaData.totalPlaytimeInSeconds;

        Debug.Log("¡Datos aplicados!");
        this.dataToLoad = null;
    }
    public void ResetSessionData()
    {
        sceneDialogueState.Clear();

        totalPlaytimeInSeconds = 0f;

        Debug.Log("SaveManager: Datos de sesión reseteados para 'New Game'.");
    }
    public void DeleteSavedData()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                File.Delete(saveFilePath);
                Debug.Log($"Archivo de guardado borrado de: {saveFilePath}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al intentar borrar el archivo de guardado: {ex.Message}");
            }
        }
        else
        {
            Debug.Log("No se encontró archivo de guardado para borrar. No se hace nada.");
        }
    }
}