using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneSpawnManager : MonoBehaviour
{
    public static string NextSpawnPoint;

    private void Awake()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        var player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            Debug.LogWarning("⚠️ No se encontró el jugador en la escena.");
            return;
        }

        // 🔹 Si no hay spawn point pendiente, no tocamos la posición del jugador.
        if (string.IsNullOrEmpty(NextSpawnPoint))
        {
            Debug.Log("➡️ Cargando escena sin punto de spawn personalizado. Manteniendo posición del jugador.");
            return;
        }

        Transform targetSpawn = null;

        // 🔸 Intentar encontrar el spawn personalizado
        var customSpawn = GameObject.Find(NextSpawnPoint);
        if (customSpawn != null)
        {
            targetSpawn = customSpawn.transform;
        }
        else
        {
            // 🔸 Si no existe, intentar con DefaultSpawn
            var defaultSpawn = GameObject.FindGameObjectWithTag("DefaultSpawn");
            if (defaultSpawn != null)
                targetSpawn = defaultSpawn.transform;
        }

        // 🔸 Si se encontró algún destino, mover al jugador
        if (targetSpawn != null)
        {
            player.transform.position = targetSpawn.position;
            Debug.Log($"✅ Jugador spawneado en: {targetSpawn.name}");
        }
        else
        {
            Debug.Log("⚠️ No se encontró punto de spawn válido. Manteniendo posición actual del jugador.");
        }

        // 🔸 Limpiar el valor para la próxima escena
        NextSpawnPoint = null;
    }
}
