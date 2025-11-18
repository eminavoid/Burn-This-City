using UnityEngine;

public class ZeppelinFloat : MonoBehaviour
{
    [Header("Movimiento vertical (flotar)")]
    public float floatAmplitude = 20f;   // Qué tan alto/bajo se mueve
    public float floatSpeed = 1f;        // Velocidad del flotado

    [Header("Movimiento horizontal")]
    public float moveSpeed = 100f;       // Velocidad hacia la izquierda
    public float resetDelay = 1f;        // Tiempo antes de volver a empezar

    [Header("Posiciones")]
    public float startX = 1000f;         // Punto inicial derecha
    public float endX = -1000f;          // Punto final izquierda

    private Vector3 startPos;

    private void Start()
    {
        // Arranca desde el punto inicial
        startPos = new Vector3(startX, transform.localPosition.y, 0);
        transform.localPosition = startPos;
    }

    private void Update()
    {
        // --- MOVIMIENTO HORIZONTAL ---
        transform.localPosition += Vector3.left * moveSpeed * Time.deltaTime;

        // Si llegó a la izquierda ⇒ reiniciar
        if (transform.localPosition.x <= endX)
            StartCoroutine(ResetPosition());

        // --- MOVIMIENTO VERTICAL (flotar) ---
        float yOffset = Mathf.Sin(Time.time * floatSpeed) * floatAmplitude;
        transform.localPosition = new Vector3(
            transform.localPosition.x,
            startPos.y + yOffset,
            transform.localPosition.z
        );
    }

    private System.Collections.IEnumerator ResetPosition()
    {
        // Evita múltiples resets
        enabled = false;

        yield return new WaitForSeconds(resetDelay);

        // Vuelve a la derecha
        transform.localPosition = startPos;

        enabled = true;
    }
}
