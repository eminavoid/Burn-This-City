using UnityEngine;
using UnityEngine.SceneManagement;

public class scenename : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log($"Escena actual: {SceneManager.GetActiveScene().name}");
    }
}
