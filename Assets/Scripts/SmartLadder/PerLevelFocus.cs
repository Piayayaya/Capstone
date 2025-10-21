using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "SmartLadder/Per Level Focus Profile", fileName = "PerLevelFocus")]
public class PerLevelFocus : ScriptableObject
{
    [Tooltip("Normalized vertical positions (0 = bottom, 1 = top), one per level index (0-based).")]
    public List<float> normalizedPerLevel = new List<float>();

    public int Count => (normalizedPerLevel != null) ? normalizedPerLevel.Count : 0;

    /// <summary>Safe getter. Clamps index and value to [0..1].</summary>
    public float Get(int index)
    {
        if (normalizedPerLevel == null || normalizedPerLevel.Count == 0) return 0f;
        index = Mathf.Clamp(index, 0, normalizedPerLevel.Count - 1);
        return Mathf.Clamp01(normalizedPerLevel[index]);
    }

    // --------- Quality-of-life: one-click autofill buttons ---------

    /// <summary>Fill with evenly spaced values between bottom and top (inclusive).</summary>
    public void AutoFill(int count, float bottom = 0f, float top = 1f)
    {
        count = Mathf.Max(1, count);
        if (normalizedPerLevel == null) normalizedPerLevel = new List<float>(count);
        normalizedPerLevel.Clear();

        if (count == 1)
        {
            normalizedPerLevel.Add(Mathf.Clamp01((bottom + top) * 0.5f));
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float t = (float)i / (count - 1);          // 0..1
            float v = Mathf.Lerp(bottom, top, t);      // bottom..top
            normalizedPerLevel.Add(Mathf.Clamp01(v));  // ensure 0..1
        }
    }

    /// <summary>Clamp all existing values to [0..1] without changing the count.</summary>
    public void ClampAll()
    {
        if (normalizedPerLevel == null) return;
        for (int i = 0; i < normalizedPerLevel.Count; i++)
            normalizedPerLevel[i] = Mathf.Clamp01(normalizedPerLevel[i]);
    }

    // Inspector gear-menu shortcuts (no custom editor needed)

    [ContextMenu("Auto Fill → 11 (0 → 1)")]
    void CM_AutoFill_11_Default() => AutoFill(11, 0f, 1f); // Easy: 10 levels + chest

    [ContextMenu("Auto Fill → 16 (0 → 1)")]
    void CM_AutoFill_16_Default() => AutoFill(16, 0f, 1f); // Normal example

    [ContextMenu("Auto Fill → 26 (0 → 1)")]
    void CM_AutoFill_26_Default() => AutoFill(26, 0f, 1f); // Hard example

    [ContextMenu("Auto Fill → 36 (0 → 1)")]
    void CM_AutoFill_36_Default() => AutoFill(36, 0f, 1f); // Advanced example

    [ContextMenu("Auto Fill → 51 (0 → 1)")]
    void CM_AutoFill_51_Default() => AutoFill(51, 0f, 1f); // Expert example (50 + chest)

    [ContextMenu("Clamp All to [0..1]")]
    void CM_Clamp() => ClampAll();

    // Safety: never auto-overwrite your values unless you click a menu
    void OnValidate() => ClampAll();
}
