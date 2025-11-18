using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [Header("Nueva canción al entrar")]
    [SerializeField] private AudioClip newClip;

    [Tooltip("Evita que se active múltiples veces")]
    [SerializeField] private bool playOnlyOnce = true;

    private bool alreadyPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (alreadyPlayed && playOnlyOnce) Destroy(gameObject);

        if (newClip != null)
        {
            AudioManager.Instance.PlayMusic(newClip);
            alreadyPlayed = true;
        }
    }
}
