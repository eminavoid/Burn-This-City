using UnityEngine;

public class MusicTrigger : MonoBehaviour
{
    [Header("Referencia al reproductor de música de la escena")]
    [SerializeField] private AudioSource musicPlayer;

    [Header("Nueva canción al entrar")]
    [SerializeField] private AudioClip newClip;

    [Tooltip("Evita que se active múltiples veces")]
    [SerializeField] private bool playOnlyOnce = true;

    private bool alreadyPlayed = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;
        if (alreadyPlayed && playOnlyOnce) return;

        if (musicPlayer != null && newClip != null && musicPlayer.clip != newClip)
        {
            musicPlayer.clip = newClip;
            musicPlayer.Play();
            alreadyPlayed = true;
        }
    }
}
