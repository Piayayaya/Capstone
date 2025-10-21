using UnityEngine;
using UnityEngine.UI;

public class ContentSizeByDifficulty : MonoBehaviour
{
    [Header("Refs")]
    public RectTransform content;     // Scroll View -> Viewport -> Content
    public ScrollRect scroll;         // (optional) to snap after resizing

    [Header("Heights (pixels)")]
    public float easyHeight = 6000f;
    public float normalHeight = 9000f;
    public float hardHeight = 12000f;
    public float advancedHeight = 15000f;
    public float expertHeight = 18000f;

    [Header("Options")]
    public bool snapToBottom = true;  // start near Level 1 after resize

    void Reset()
    {
        // Auto-fill when added to Content
        content = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (!content) content = GetComponent<RectTransform>();
        Apply();
    }

    public void Apply()
    {
        if (!content) return;

        float h = GetHeight(SmartLadderSession.SelectedDifficulty);

        var size = content.sizeDelta;
        size.y = h;
        content.sizeDelta = size;

        if (snapToBottom && scroll)
        {
            // 0 = bottom, 1 = top
            scroll.verticalNormalizedPosition = 0f;
            scroll.velocity = Vector2.zero;
        }
    }

    float GetHeight(LadderDifficulty d)
    {
        switch (d)
        {
            case LadderDifficulty.Easy: return easyHeight;
            case LadderDifficulty.Normal: return normalHeight;
            case LadderDifficulty.Hard: return hardHeight;
            case LadderDifficulty.Advanced: return advancedHeight;
            case LadderDifficulty.Expert: return expertHeight;
            default: return easyHeight;
        }
    }
}
