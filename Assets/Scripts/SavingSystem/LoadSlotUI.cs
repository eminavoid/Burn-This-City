using UnityEngine;
using UnityEngine.UI;
using TMPro; // Usa esto si tienes TextMeshPro
using System.IO;

/// <summary>
/// Gestiona la visualización de un único slot de guardado.
/// Al activarse (OnEnable), comprueba si los archivos de guardado
/// existen y actualiza la UI correspondientemente.
/// </summary>
public class LoadSlotUI : MonoBehaviour
{
    [Header("Configuración de Guardado")]
    [Tooltip("El nombre base del archivo a cargar (debe coincidir con el de SaveManager, ej: 'burnthiscity')")]
    private string saveFileBaseName;

    [Header("UI Elements")]
    [Tooltip("El objeto a mostrar si el slot está vacío.")]
    [SerializeField] private GameObject emptySlotVisuals;
    [Tooltip("El objeto que contiene los datos si el slot está lleno.")]
    [SerializeField] private GameObject dataSlotVisuals;

    [Header("Data Visuals (Hijos de DataSlotVisuals)")]
    [Tooltip("Componente RawImage para mostrar la screenshot.")]
    [SerializeField] private RawImage screenshotImage;
    [Tooltip("Texto para el tiempo de juego.")]
    [SerializeField] private TextMeshProUGUI playtimeText;
    [Tooltip("Texto para el nombre de la escena.")]
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [Tooltip("Texto para la fecha y hora.")]
    [SerializeField] private TextMeshProUGUI timestampText;

    /// <summary>
    /// Se llama CADA VEZ que el objeto se activa.
    /// Perfecto para refrescar la UI del menú de carga.
    /// </summary>
    private void OnEnable()
    {
        saveFileBaseName = SaveManager.Instance.saveFileBaseName;
        PopulateSlot();

    }

    /// <summary>
    /// Comprueba si existe el archivo de guardado y puebla la UI.
    /// </summary>
    private void PopulateSlot()
    {
        // --- 1. Validar Referencias Clave ---
        if (emptySlotVisuals == null || dataSlotVisuals == null)
        {
            Debug.LogError($"LoadSlotUI: '{name}' le faltan las referencias a 'emptySlotVisuals' o 'dataSlotVisuals'. Por favor, asígnalas en el Inspector.");
            return;
        }

        // --- 2. Definir Rutas ---
        string persistentPath = Application.persistentDataPath;
        string jsonPath = Path.Combine(persistentPath, saveFileBaseName + ".json");
        string pngPath = Path.Combine(persistentPath, saveFileBaseName + ".png");

        // --- 3. Comprobar si existe el guardado ---
        if (!File.Exists(jsonPath))
        {
            // No existe guardado
            emptySlotVisuals.SetActive(true);
            dataSlotVisuals.SetActive(false);
            return;
        }

        // --- 4. Existe guardado ---
        // ¡Este es el fix! Nos aseguramos de ocultar 'empty' y mostrar 'data'
        emptySlotVisuals.SetActive(false);
        dataSlotVisuals.SetActive(true);

        // --- 5. Cargar y Poblar Datos ---
        try
        {
            // Cargar datos JSON
            string json = File.ReadAllText(jsonPath);
            GameData data = JsonUtility.FromJson<GameData>(json);

            // Poblar Textos (Metadata)
            if (playtimeText != null)
            {
                System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(data.metaData.totalPlaytimeInSeconds);
                playtimeText.text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                                timeSpan.Hours,
                                                timeSpan.Minutes,
                                                timeSpan.Seconds);
            }

            if (timestampText != null)
            {
                System.DateTime saveTime = System.DateTime.Parse(data.metaData.saveTimestamp);
                timestampText.text = saveTime.ToString("g"); // Formato corto
            }

            if (sceneNameText != null)
            {
                sceneNameText.text = data.sceneName;
            }

            // Cargar Screenshot (PNG/JPG)
            if (screenshotImage != null)
            {
                if (File.Exists(pngPath))
                {
                    byte[] fileData = File.ReadAllBytes(pngPath);
                    Texture2D tex = new Texture2D(2, 2);
                    if (tex.LoadImage(fileData))
                    {
                        screenshotImage.texture = tex;
                        screenshotImage.color = Color.white;
                    }
                }
                else
                {
                    screenshotImage.texture = null;
                    screenshotImage.color = Color.black;
                }
            }
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"Error al cargar y poblar el slot: {ex.Message}");
            // Si el JSON está corrupto o algo falla, lo mostramos como vacío
            emptySlotVisuals.SetActive(true);
            dataSlotVisuals.SetActive(false);
        }
    }
}