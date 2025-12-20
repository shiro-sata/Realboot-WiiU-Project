using UnityEngine;
using UnityEngine.UI;

public class BackgroundLayer : MonoBehaviour
{
    [Header("Components")]
    public Image targetImage;
    public Material transitionMaterial; 

    [Header("State")]
    public string currentBgName = "";
    
    private Material runtimeMaterial;

    void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        
        if (transitionMaterial != null)
        {
            runtimeMaterial = new Material(transitionMaterial);
        }

        // Initialize as invisible/empty
        SetOpacity(0f);
    }

    public void LoadBackground(string bgName)
    {
        // Handle "NONE" or empty
        if (string.IsNullOrEmpty(bgName) || bgName == "NONE")
        {
            SetOpacity(0f);
            currentBgName = "";
            return;
        }

        // Load Sprite
        Sprite bgSprite = Resources.Load<Sprite>("bg/" + bgName);

        if (bgSprite != null)
        {
            targetImage.sprite = bgSprite;
            
            // Reset material to default
            targetImage.material = null; 
            
            // Make visible (Alpha 255 / 1.0f)
            SetOpacity(1f);
            
            currentBgName = bgName;
        }
        else
        {
            Debug.LogWarning("[BG] Could not find background: bg/" + bgName);
            // Safety: hide if not found
            SetOpacity(0f);
        }
    }

    // Helper to change Alpha
    public void SetOpacity(float alpha)
    {
        if (targetImage == null) return;

        Color c = targetImage.color;
        c.a = alpha;
        targetImage.color = c;
    }

    // Apply a mask with given name and range value
    public void ApplyMask(string maskName, float value)
    {
        if (runtimeMaterial == null) return;

        Texture maskTex = Resources.Load<Texture>("mask/" + maskName);
        if (maskTex != null)
        {
            targetImage.material = runtimeMaterial;
            runtimeMaterial.SetTexture("_MaskTex", maskTex);
            runtimeMaterial.SetFloat("_Range", value);
        }
    }
}