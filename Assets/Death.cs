using UnityEngine;

public class Death : MonoBehaviour
{
    [SerializeField] private Canvas _canvas;

    void Start()
    {
        // 1. Suscribirse al evento OnDied, que se dispara solo cuando HP llega a 0.
        // 2. Nota que usamos "die" (sin paréntesis)
        SurvivabilityManager.Instance.OnStatZero.AddListener(OnStatZeroHandler);
    }

    // 3. El método debe ser 'void' y no aceptar argumentos, 
    //    para coincidir con la firma del evento 'OnDied'.
    public void OnStatZeroHandler(SurvivabilityStat stat)
    {
        if (stat == SurvivabilityStat.HP || stat == SurvivabilityStat.Sanity)
        {
            Die();
        }
    }

    private void Die()
    {
        _canvas.gameObject.SetActive(true);
    }

    // Opcional: Buena práctica para desuscribirse del evento
    void OnDestroy()
    {
        // Si la instancia aún existe, nos desuscribimos para evitar errores.
        if (SurvivabilityManager.Instance != null)
        {
            SurvivabilityManager.Instance.OnStatZero.RemoveListener(OnStatZeroHandler);
        }
    }
}
