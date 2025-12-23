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
    public float characterFadeInDuration = 0.05f;
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
    
    // Internal display logic with pre-calculation
    private IEnumerator DisplayTextInternal(string name, string body, AudioClip voiceClip, float? customSpeed)
    {
        isDisplaying = true;
        isTextComplete = false;
        skipRequested = false;
        
        // Set name
        nameText.text = name;
        
        // Calculate typing speed based on voice length
        float typingSpeed = customSpeed ?? defaultTypingSpeed;
        if (voiceClip != null)
        {
            // Adapt speed to match voice duration
            float voiceDuration = voiceClip.length;
            int characterCount = body.Length;
            
            // Calculate speed to finish text when voice endsç
            if (characterCount > 0)
            {
                typingSpeed = (voiceDuration * 0.95f) / characterCount;
                typingSpeed = Mathf.Clamp(typingSpeed, 0.01f, 0.1f);
            }
        }
        
        currentTypingSpeed = typingSpeed;
        
        // Pre-calculate character positions
        bodyText.text = body;
        bodyText.ForceMeshUpdate();
        TMP_TextInfo textInfo = bodyText.textInfo;
        int totalCharacters = textInfo.characterCount;
        
        // Hide all characters initially
        Color32[] originalColors = new Color32[textInfo.meshInfo.Length];
        for (int i = 0; i < textInfo.meshInfo.Length; i++)
        {
            originalColors[i] = textInfo.meshInfo[i].colors32[0];
        }
        
        // Make all characters invisible
        for (int i = 0; i < totalCharacters; i++)
        {
            if (!textInfo.characterInfo[i].isVisible) continue;
            
            int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
            int vertexIndex = textInfo.characterInfo[i].vertexIndex;
            Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
            
            colors[vertexIndex + 0].a = 0;
            colors[vertexIndex + 1].a = 0;
            colors[vertexIndex + 2].a = 0;
            colors[vertexIndex + 3].a = 0;
        }
        
        bodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        
        // Character-by-character reveal with fade-in
        float timer = 0f;
        int lastVisibleChar = -1;
        
        while (lastVisibleChar < totalCharacters - 1 && !skipRequested)
        {
            timer += Time.deltaTime;
            
            // Calculate which character should be visible now
            int targetChar = Mathf.FloorToInt(timer / typingSpeed);
            targetChar = Mathf.Min(targetChar, totalCharacters - 1);
            
            // Fade in new characters
            for (int i = lastVisibleChar + 1; i <= targetChar; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;
                
                StartCoroutine(FadeInCharacter(i, textInfo));
            }
            
            lastVisibleChar = targetChar;
            
            yield return null;
        }
        
        // If skipped, instantly reveal all
        if (skipRequested)
        {
            for (int i = 0; i < totalCharacters; i++)
            {
                if (!textInfo.characterInfo[i].isVisible) continue;
                
                int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
                int vertexIndex = textInfo.characterInfo[i].vertexIndex;
                Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
                
                colors[vertexIndex + 0].a = 255;
                colors[vertexIndex + 1].a = 255;
                colors[vertexIndex + 2].a = 255;
                colors[vertexIndex + 3].a = 255;
            }
            
            bodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
        }
        
        isTextComplete = true;
        isDisplaying = false;
    }
    
    // Fade in individual character
    private IEnumerator FadeInCharacter(int charIndex, TMP_TextInfo textInfo)
    {
        if (!textInfo.characterInfo[charIndex].isVisible) yield break;
        
        int materialIndex = textInfo.characterInfo[charIndex].materialReferenceIndex;
        int vertexIndex = textInfo.characterInfo[charIndex].vertexIndex;
        Color32[] colors = textInfo.meshInfo[materialIndex].colors32;
        
        float elapsed = 0f;
        
        while (elapsed < characterFadeInDuration)
        {
            elapsed += Time.deltaTime;
            byte alpha = (byte)(255 * Mathf.Clamp01(elapsed / characterFadeInDuration));
            
            colors[vertexIndex + 0].a = alpha;
            colors[vertexIndex + 1].a = alpha;
            colors[vertexIndex + 2].a = alpha;
            colors[vertexIndex + 3].a = alpha;
            
            bodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
            
            yield return null;
        }
        
        // Ensure fully visible
        colors[vertexIndex + 0].a = 255;
        colors[vertexIndex + 1].a = 255;
        colors[vertexIndex + 2].a = 255;
        colors[vertexIndex + 3].a = 255;
        
        bodyText.UpdateVertexData(TMP_VertexDataUpdateFlags.Colors32);
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