using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DialogueSystem : MonoBehaviour
{
    [Header("UI References")]
    public TMP_Text nameText;
    public TMP_Text bodyText;
    public CanvasGroup textBoxCanvasGroup;

    [Header("Animation Settings")]
    public float textFadeOutDuration = 0.2f;
    public float textBoxFadeDuration = 0.3f;
    public float defaultTypingSpeed = 0.04f;
    
    // Public state flags
    [HideInInspector] public bool isTextComplete = false;
    [HideInInspector] public bool isDisplaying = false;
    [HideInInspector] public bool isTextBoxVisible = false;
    
    // Private state
    private Coroutine currentDisplayCoroutine;
    private Coroutine currentTextBoxCoroutine;
    private bool skipRequested = false;
    private float currentTypingSpeed;
    
    void Awake()
    {
        if (textBoxCanvasGroup != null)
        {
            textBoxCanvasGroup.alpha = 0f;
            isTextBoxVisible = false;
        }
        
        currentTypingSpeed = defaultTypingSpeed;
    }
    
    // Request skip during text display
    public void RequestSkip()
    {
        if (isDisplaying && !isTextComplete)
        {
            skipRequested = true;
        }
    }
    
    // Main display function with full control
    public IEnumerator DisplayText(string name, string body, AudioClip voiceClip = null, float? customSpeed = null)
    {
        // Stop any current display
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
        }
        
        currentDisplayCoroutine = StartCoroutine(DisplayTextInternal(name, body, voiceClip, customSpeed));
        yield return currentDisplayCoroutine;
    }

    // Internal display logic with classic typing effect
    private IEnumerator DisplayTextInternal(string name, string body, AudioClip voiceClip, float? customSpeed)
    {
        isDisplaying = true;
        isTextComplete = false;
        skipRequested = false;

        nameText.text = name;
        bodyText.text = body;
        bodyText.maxVisibleCharacters = 0;
        
        // Force mesh update to get character count
        bodyText.ForceMeshUpdate();

        // Calculate typing speed
        float typingSpeed = customSpeed ?? defaultTypingSpeed;
        if (voiceClip != null && body.Length > 0)
        {
            typingSpeed = (voiceClip.length * 0.95f) / body.Length;
            typingSpeed = Mathf.Clamp(typingSpeed, 0.01f, 0.1f);
        }
        currentTypingSpeed = typingSpeed;

        TMP_TextInfo textInfo = bodyText.textInfo;
        int totalCharacters = textInfo.characterCount;

        // Classic typing effect loop
        int currentChar = 0;
        float timer = 0f;

        while (currentChar < totalCharacters && !skipRequested)
        {
            timer += Time.deltaTime;
            
            int targetChar = Mathf.FloorToInt(timer / typingSpeed);
            targetChar = Mathf.Min(targetChar, totalCharacters);
            
            if (targetChar > currentChar)
            {
                currentChar = targetChar;
                bodyText.maxVisibleCharacters = currentChar;
            }
            
            yield return null;
        }

        bodyText.maxVisibleCharacters = totalCharacters;
        
        if (skipRequested)
        {
            yield return null; 
        }

        isTextComplete = true;
        isDisplaying = false;
    }
    
    // Clear text with fade out
    public IEnumerator ClearText()
    {
        if (string.IsNullOrEmpty(bodyText.text)) yield break;
        
        float elapsed = 0f;
        CanvasGroup textCanvasGroup = bodyText.GetComponent<CanvasGroup>();
        
        // Add CanvasGroup if not present
        if (textCanvasGroup == null)
        {
            textCanvasGroup = bodyText.gameObject.AddComponent<CanvasGroup>();
        }
        
        float startAlpha = textCanvasGroup.alpha;
        
        while (elapsed < textFadeOutDuration)
        {
            elapsed += Time.deltaTime;
            textCanvasGroup.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / textFadeOutDuration);
            yield return null;
        }
        
        bodyText.text = "";
        nameText.text = "";
        textCanvasGroup.alpha = 1f;
    }
    
    // Show text box with fade in
    public IEnumerator ShowTextBox()
    {
        if (textBoxCanvasGroup == null || isTextBoxVisible) yield break;
        
        if (currentTextBoxCoroutine != null)
        {
            StopCoroutine(currentTextBoxCoroutine);
        }
        
        currentTextBoxCoroutine = StartCoroutine(FadeTextBox(true));
        yield return currentTextBoxCoroutine;
    }
    
    // Hide text box with fade out
    public IEnumerator HideTextBox()
    {
        if (textBoxCanvasGroup == null || !isTextBoxVisible) yield break;
        
        if (currentTextBoxCoroutine != null)
        {
            StopCoroutine(currentTextBoxCoroutine);
        }
        
        currentTextBoxCoroutine = StartCoroutine(FadeTextBox(false));
        yield return currentTextBoxCoroutine;
    }
    
    // Fade text box
    private IEnumerator FadeTextBox(bool fadeIn)
    {
        float startAlpha = textBoxCanvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;
        float elapsed = 0f;
        
        while (elapsed < textBoxFadeDuration)
        {
            elapsed += Time.deltaTime;
            textBoxCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / textBoxFadeDuration);
            yield return null;
        }
        
        textBoxCanvasGroup.alpha = targetAlpha;
        isTextBoxVisible = fadeIn;
    }
    
    // Reset to clean state
    public void Reset()
    {
        if (currentDisplayCoroutine != null)
        {
            StopCoroutine(currentDisplayCoroutine);
            currentDisplayCoroutine = null;
        }
        
        bodyText.text = "";
        nameText.text = "";
        isDisplaying = false;
        isTextComplete = false;
        skipRequested = false;
    }
}