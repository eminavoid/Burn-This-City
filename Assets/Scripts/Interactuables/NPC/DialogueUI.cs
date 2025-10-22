using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;

public class DialogueUI : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Shows the NPC’s name")]
    public TMP_Text npcNameText;

    [Tooltip("Shows what the NPC actually says")]
    public TMP_Text dialogueText;

    [Tooltip("Exactly 3 buttons here")]
    public Button[] choiceButtons; // size = 3
    
    [Header("Typing Settings")]
    [Tooltip("Delay between each character")]
    [SerializeField] private float typingSpeed = 0.03f;

    [Tooltip("Play sound every N letters")]
    [SerializeField] private int lettersPerSound = 2;

    [Header("Audio (typing)")]
    [SerializeField] private AudioClip typingSFX;
    [SerializeField] private AudioClip stopTypingSFX;
    [SerializeField] private AudioSource audioSource;
    
    // Estado interno
    private Coroutine typingCoroutine;
    private bool isTyping = false;
    private string fullText;
    private List<(string optionText, int optionIndex)> _options;
    
    private void Update()
    {
        // Permite saltar el efecto de tipeo presionando espacio o clic izquierdo
        if (isTyping && (Input.GetKeyDown(KeyCode.Space)))
        {
            SkipTyping();
        }
    }

    public void Show(string npcName, string text, List<(string optionText, int optionIndex)> options)
    {
        gameObject.SetActive(true);
        npcNameText.text = npcName;
        _options = options;

        // Detener tipeo previo si existe
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeText(text));

        // Configurar los botones (los desactivamos / bloqueamos hasta que termine el tipeo)
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            var btn = choiceButtons[i];
            bool has = i < _options.Count;
            btn.gameObject.SetActive(false);
            btn.onClick.RemoveAllListeners();

            // Inicialmente no interactuables hasta que termine de tipearse
            btn.interactable = false;

            if (has)
            {
                var label = btn.GetComponentInChildren<TMP_Text>();
                label.text = _options[i].optionText;

                int idx = _options[i].optionIndex;
                btn.onClick.AddListener(() =>
                {
                    var runner = DialogueRunner.Instance ?? FindObjectOfType<DialogueRunner>();
                    runner?.Choose(idx);
                });
            }
        }
    }
    
    private IEnumerator TypeText(string text)
    {
        isTyping = true;
        fullText = text;
        dialogueText.text = "";

        for (int i = 0; i < text.Length; i++)
        {
            dialogueText.text += text[i];

            // Reproducir sonido cada N letras (usar audioSource si está, si no AudioManager)
            if (typingSFX != null && (i % lettersPerSound == 0)) {
                if (audioSource != null) audioSource.PlayOneShot(typingSFX);
                else if (AudioManager.Instance != null) AudioManager.Instance.PlaySFX(typingSFX, 0.5f);
            }
            
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
        RevealChoiceButtons();
    }
    
    private void SkipTyping()
    {
        if (!isTyping) return;

        StopCoroutine(typingCoroutine);
        dialogueText.text = fullText;
        isTyping = false;

        // Habilitar botones al skipear
        RevealChoiceButtons();
    }

    public void Hide()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        gameObject.SetActive(false);
        isTyping = false;

        // limpiar listeners para evitar fugas
        foreach (var btn in choiceButtons)
        {
            btn.onClick.RemoveAllListeners();
        }
    }
    
    private void RevealChoiceButtons()
    {
        StopTypingAudio();
        
        for (int i = 0; i < choiceButtons.Length; i++)
        {
            var btn = choiceButtons[i];
            bool has = i < _options.Count;
            btn.gameObject.SetActive(has);
            btn.interactable = has;
        }
    }
    
    public void StopTypingAudio()
    {
        if (audioSource != null)
        {
            audioSource.Stop();
            audioSource.PlayOneShot(stopTypingSFX);
        }
        else if (AudioManager.Instance != null)
        {
            AudioManager.Instance.StopSFX();
            AudioManager.Instance.PlaySFX(stopTypingSFX, .5f);
        }
    }
}