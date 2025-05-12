using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public int dayCount = 0;
    public float globalTime = 0f;
    public bool isGameOver = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (isGameOver) return;

        globalTime += Time.deltaTime;
        // logica de dia/noche, eventos, etc.
    }

    public void EndGame()
    {
        isGameOver = true;
        Debug.Log("Game Over");
    }
}
