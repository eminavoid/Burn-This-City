using UnityEngine;
using UnityEngine.SceneManagement;

public class BackstoryManager : MonoBehaviour
{
    [Tooltip("Root node for your backstory")]
    public DialogueNode startingNode;

    [Tooltip("Which DialogueRunner to use (usually DialogueManager)")]
    public DialogueRunner dialogueRunner;


    private void Start()
    {
        dialogueRunner.Begin(startingNode, null);
        DialogueRunner.DialogueEnded += OnBackstoryComplete;
    }

    private void OnBackstoryComplete()
    {
        DialogueRunner.DialogueEnded -= OnBackstoryComplete;
        if (ScreenFader.Instance != null)
        {
            ScreenFader.Instance.FadeOutAndLoadScene("Camp");
        }
        else
        {
            Debug.LogWarning("No SceneController found—did you forget to add it?");
        }
    }
}