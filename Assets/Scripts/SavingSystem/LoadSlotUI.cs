using UnityEngine;
using UnityEngine.UI;
using TMPro; // Usa esto si tienes TextMeshPro
using System.IO;

public class LoadSlotUI : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private RawImage screenshotImage; // <-- USA RAWIMAGE, no Image
    [SerializeField] private TextMeshProUGUI playtimeText;
    [SerializeField] private TextMeshProUGUI sceneNameText;
    [SerializeField] private TextMeshProUGUI timestampText;
    [SerializeField] private GameObject emptySlotVisuals; // Objeto a mostrar si no hay guardado
    [SerializeField] private GameObject dataSlotVisuals;  // Objeto a mostrar si hay guardado

    /// <summary>
    /// Llama a este método para poblar el slot.
    /// </summary>
    public void CheckAndPopulateSlot(string baseSaveName)
    {
        string persistentPath = Application.persistentDataPath;
        string jsonPath = Path.Combine(persistentPath, baseSaveName + ".json");
        string pngPath = Path.Combine(persistentPath, baseSaveName + ".png");

        if (!File.Exists(jsonPath))
        {
            // No existe guardado, muestra el visual de "slot vacío"
            emptySlotVisuals.SetActive(true);
            dataSlotVisuals.SetActive(false);
            return;
        }

        // Existe guardado, muestra los datos
        emptySlotVisuals.SetActive(false);
        dataSlotVisuals.SetActive(true);

        // 1. Cargar datos JSON
        string json = File.ReadAllText(jsonPath);
        GameData data = JsonUtility.FromJson<GameData>(json);

        // 2. Poblar Textos (Metadata)
        // Formatear el tiempo de juego
        System.TimeSpan timeSpan = System.TimeSpan.FromSeconds(data.metaData.totalPlaytimeInSeconds);
        playtimeText.text = string.Format("{0:D2}h:{1:D2}m:{2:D2}s",
                                        timeSpan.Hours,
                                        timeSpan.Minutes,
                                        timeSpan.Seconds);

        // Formatear el timestamp
        System.DateTime saveTime = System.DateTime.Parse(data.metaData.saveTimestamp);
        timestampText.text = saveTime.ToString("g"); // Formato corto de fecha y hora

        // (Aquí puedes usar el truco del diccionario para traducir el nombre de la escena)
        sceneNameText.text = data.sceneName;

        // 3. Cargar Screenshot (PNG/JPG)
        if (File.Exists(pngPath))
        {
            // Lee los bytes del archivo
            byte[] fileData = File.ReadAllBytes(pngPath);

            // Crea una nueva textura y cárgale los bytes
            Texture2D tex = new Texture2D(2, 2); // El tamaño inicial no importa
            if (tex.LoadImage(fileData)) // LoadImage detecta si es JPG o PNG
            {
                screenshotImage.texture = tex;
                screenshotImage.color = Color.white; // Asegúrate de que sea visible
            }
        }
        else
        {
            // No se encontró screenshot, muestra un color negro
            screenshotImage.texture = null;
            screenshotImage.color = Color.black;
        }
    }
}