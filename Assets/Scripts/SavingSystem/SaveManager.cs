using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SaveManager : MonoBehaviour
{
    public static SaveManager Instance { get; private set; }

    private float totalPlaytimeInSeconds = 0f;

    [Tooltip("El nombre base para los archivos de guardado, sin extensión.")]
    [SerializeField] private string saveFileBaseName = "save";
    private string saveFilePath_JSON;
    private string saveFilePath_PNG;

    private Dictionary<int, DialogueNode> sceneDialogueState = new Dictionary<int, DialogueNode>();

    private GameData dataToLoad = null;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        string persistentPath = Application.persistentDataPath;
        saveFilePath_JSON = Path.Combine(persistentPath, saveFileBaseName + ".json");
        saveFilePath_PNG = Path.Combine(persistentPath, saveFileBaseName + ".png");

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
        Debug.Log("Iniciando guardado...");
        StartCoroutine(SaveGameCoroutine());
    }
    private IEnumerator SaveGameCoroutine()
    {
        yield return new WaitForEndOfFrame();

        // 2. CAPTURAR LA PANTALLA (Textura completa)
        Texture2D fullScreenTexture = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
        fullScreenTexture.ReadPixels(new Rect(0, 0, Screen.width, Screen.height), 0, 0);
        fullScreenTexture.Apply();

        // 3. REDIMENSIONAR EN LA GPU
        int thumbWidth = 640;
        int thumbHeight = 360;
        RenderTexture rt = RenderTexture.GetTemporary(thumbWidth, thumbHeight, 0);

        // Copia la textura completa a la RenderTexture pequeña (blit)
        Graphics.Blit(fullScreenTexture, rt);

        // Lee la RenderTexture pequeña de vuelta a una Texture2D
        RenderTexture.active = rt;
        Texture2D thumbnailTexture = new Texture2D(thumbWidth, thumbHeight, TextureFormat.RGB24, false);
        thumbnailTexture.ReadPixels(new Rect(0, 0, thumbWidth, thumbHeight), 0, 0);
        thumbnailTexture.Apply();

        // Limpieza de memoria
        RenderTexture.active = null;
        RenderTexture.ReleaseTemporary(rt);
        Destroy(fullScreenTexture); // Ya no necesitamos la grande

        // 4. CODIFICAR A JPG Y GUARDAR SCREENSHOT
        byte[] screenshotBytes = thumbnailTexture.EncodeToJPG(75);
        Destroy(thumbnailTexture); // Ya no necesitamos la pequeña
        File.WriteAllBytes(saveFilePath_PNG, screenshotBytes);

        // 5. RECOLECTAR Y GUARDAR DATOS (JSON)
        GameData gameData = new GameData();

        // 5a. Metadata
        gameData.metaData.saveTimestamp = System.DateTime.Now.ToString("o");
        gameData.metaData.totalPlaytimeInSeconds = this.totalPlaytimeInSeconds;
        gameData.metaData.gameVersion = Application.version;

        // 5b. Escena y Jugador
        gameData.sceneName = SceneManager.GetActiveScene().name;
        PlayerMovement2D player = FindFirstObjectByType<PlayerMovement2D>();
        if (player != null)
        {
            gameData.playerData.playerPosX = player.transform.position.x;
            gameData.playerData.playerPosY = player.transform.position.y;
        }

        // 5c. Survivability
        gameData.playerData.currentHP = SurvivabilityManager.Instance.Get(SurvivabilityStat.HP);
        gameData.playerData.currentSanity = SurvivabilityManager.Instance.Get(SurvivabilityStat.Sanity);

        // 5d. Stats
        gameData.statsData.statEntries = StatManager.Instance.GetStatEntries();

        // 5e. Inventario (Traducción Item -> ItemID)
        gameData.inventoryData.modules = new List<SerializableModuleState>();
        foreach (var module in InventoryManager.Instance.Modules)
        {
            var newSavedModule = new SerializableModuleState();
            foreach (var slot in module.slots)
            {
                var newSavedSlot = new SerializableSlotState();
                if (slot.item != null)
                {
                    newSavedSlot.itemID = slot.item.name;
                    newSavedSlot.amount = slot.amount;
                }
                newSavedModule.slots.Add(newSavedSlot);
            }
            gameData.inventoryData.modules.Add(newSavedModule);
        }

        // 5f. Diálogos
        gameData.dialogueData.nodeStates = new List<NpcDialogueState>();
        foreach (var kvp in sceneDialogueState)
        {
            gameData.dialogueData.nodeStates.Add(new NpcDialogueState
            {
                npcID = kvp.Key,
                nodeID = (kvp.Value != null) ? kvp.Value.name : null
            });
        }

        // 6. SERIALIZAR Y ESCRIBIR EN DISCO (JSON)
        string json = JsonUtility.ToJson(gameData, true);
        File.WriteAllText(saveFilePath_JSON, json);

        Debug.Log($"Partida y Screenshot guardados en: {Application.persistentDataPath}");
    }

    public void LoadGame()
    {
        // (Modificado para usar la nueva variable de ruta)
        if (!File.Exists(saveFilePath_JSON))
        {
            Debug.LogWarning("No se encontró archivo de guardado (.json).");
            return;
        }

        Debug.Log("Cargando partida...");
        string json = File.ReadAllText(saveFilePath_JSON);
        GameData gameData = JsonUtility.FromJson<GameData>(json);

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
        // Borrar el JSON
        if (File.Exists(saveFilePath_JSON))
        {
            try
            {
                File.Delete(saveFilePath_JSON);
                Debug.Log($"Archivo JSON borrado: {saveFilePath_JSON}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al borrar JSON: {ex.Message}");
            }
        }
        else
        {
            Debug.Log("No se encontró archivo JSON para borrar.");
        }

        // Borrar el Screenshot
        if (File.Exists(saveFilePath_PNG))
        {
            try
            {
                File.Delete(saveFilePath_PNG);
                Debug.Log($"Archivo PNG borrado: {saveFilePath_PNG}");
            }
            catch (System.Exception ex)
            {
                Debug.LogError($"Error al borrar PNG: {ex.Message}");
            }
        }
    }
}