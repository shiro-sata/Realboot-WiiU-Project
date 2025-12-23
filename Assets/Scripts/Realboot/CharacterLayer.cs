using UnityEngine;
using UnityEngine.UI;

public class CharacterLayer : MonoBehaviour
{
    [Header("Components")]
    public Image targetImage;
    public RectTransform rectTransform;

    [Header("State")]
    public string currentCharaName = "";
    public int layerID = 0;

    void Awake()
    {
        if (targetImage == null) targetImage = GetComponent<Image>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        
        // Hide by default
        SetOpacity(0f);
    }

    public void LoadCharacter(string charaName)
    {
        if (string.IsNullOrEmpty(charaName) || charaName == "NONE")
        {
            SetOpacity(0f);
            currentCharaName = "";
            return;
        }

        // Load Sprite from Resources/cha/
        Sprite charSprite = Resources.Load<Sprite>("cha/" + charaName);

        if (charSprite != null)
        {
            targetImage.sprite = charSprite;
            targetImage.SetNativeSize(); // Resize rect to sprite size
            SetOpacity(1f);
            currentCharaName = charaName;
        }
        else
        {
            Debug.LogWarning("[CHARA] Could not find chara: cha/" + charaName);
            SetOpacity(0f);
        }
    }

    public void SetOpacity(float alpha)
    {
        if (targetImage == null) return;
        Color c = targetImage.color;
        c.a = alpha;
        targetImage.color = c;
    }

    public void SetPosition(float x, float y)
    {
        if (rectTransform != null)
        {
            rectTransform.anchoredPosition = new Vector2(x, y);
        }
    }
}