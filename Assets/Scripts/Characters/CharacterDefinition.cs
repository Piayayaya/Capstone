using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDef", menuName = "BrainyMe/Character")]
public class CharacterDefinition : ScriptableObject
{
    [Tooltip("Unique id used in saves (e.g., 'hootie', 'poppi')")]
    public string id;

    public string displayName;
    public Sprite previewSprite;

    [Header("Selling")]
    [Tooltip("Original price (optional; can be used to compute refund)")]
    public int originalPrice = 0;

    [Range(0f, 1f)] public float refundPercent = 0.5f;

    [Tooltip("If > 0, overrides computed refund. Leave 0 to use refundPercent * originalPrice")]
    public int overrideSellPrice = 0;

    public int GetSellPrice()
    {
        if (overrideSellPrice > 0) return overrideSellPrice;
        return Mathf.Max(0, Mathf.RoundToInt(originalPrice * refundPercent));
    }
}
