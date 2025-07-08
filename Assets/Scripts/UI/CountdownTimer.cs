using TMPro;
using UnityEngine;
using UnityEngine.Events;

public class CountdownTimer : MonoBehaviour
{
    [Tooltip("Starting duration, in seconds")]
    public float duration = 600f; // 10 minutes = 600 seconds
    [Tooltip("The TMP_Text that displays the remaining time")]
    public TMP_Text timerText;
    [Tooltip("Fired once when the timer reaches zero")]
    public UnityEvent onTimerEnd;

    private float remainingTime;
    private bool running;

    private void Start()
    {
        ResetTimer();
    }
    private void OnEnable()
    {
        DialogueRunner.DialogueStarted += () => running = false;
        DialogueRunner.DialogueEnded += () => running = true;
    }

    private void OnDisable()
    {
        DialogueRunner.DialogueStarted -= () => running = false;
        DialogueRunner.DialogueEnded -= () => running = true;
    }
    public void ResetTimer()
    {
        remainingTime = duration;
        running = true;
        UpdateTimerText();
    }
    private void Update()
    {
        if (!running) return;

        remainingTime -= Time.deltaTime;
        if (remainingTime <= 0f)
        {
            remainingTime = 0f;
            running = false;
            UpdateTimerText();
            onTimerEnd?.Invoke();
        }
        else
        {
            UpdateTimerText();
        }
    }
    private void UpdateTimerText()
    {
        int minutes = Mathf.FloorToInt(remainingTime / 60f);
        int seconds = Mathf.FloorToInt(remainingTime % 60f);
        timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
    }
}
