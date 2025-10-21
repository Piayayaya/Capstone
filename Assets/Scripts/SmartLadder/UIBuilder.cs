using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

[DisallowMultipleComponent]
public class UIBuilder : MonoBehaviour
{
    [Header("Prefabs (each item has children: 'Question' (circle), 'Number' (TMP)")]
    public GameObject oddPrefab;
    public GameObject evenPrefab;
    public GameObject chestPrefab;            // optional

    [Header("Scroll View")]
    public ScrollRect scroll;                 // drag the Scroll View (has ScrollRect)
    public RectTransform content;             // drag Scroll View / Viewport / Content

    [Header("Layout")]
    [Min(1)] public int totalLevels = 10;
    public float bottomPadding = 80f;         // where level 1 sits above bottom
    public float ySpacing = 480f;
    public float topPadding = 600f;        // space above last level
    public float oddX = -275f;
    public float evenX = 310f;

    [Header("Build Options")]
    public bool generateOnStart = true;
    public bool forceBottomPivot = true;     // make Content pivot/anchors bottom
    public bool snapToLevel1 = true;     // scroll to bottom after build

    [Header("Generated (index 0 == level 1)")]
    public List<RectTransform> platforms = new();  // the circle of each level

    const string kPlatform = "Question";
    const string kNumber = "Number";

    void Start()
    {
        if (generateOnStart) Build();
    }

    public void Build()
    {
        if (!content || !scroll)
        {
            Debug.LogError("SimpleLadderBuilder: Assign ScrollRect and Content in Inspector.");
            return;
        }

        // 1) Make Content use a bottom pivot/anchors (so +Y goes up from bottom)
        if (forceBottomPivot)
        {
            content.anchorMin = new Vector2(0.5f, 0f);
            content.anchorMax = new Vector2(0.5f, 0f);
            content.pivot = new Vector2(0.5f, 0f);
            content.anchoredPosition = new Vector2(content.anchoredPosition.x, 0f);
        }

        // 2) Clear previous items
        ClearGenerated();
        platforms.Clear();

        // 3) Spawn levels (anchoredPosition from bottom)
        for (int i = 1; i <= totalLevels; i++)
        {
            bool isOdd = (i % 2 == 1);
            var prefab = isOdd ? oddPrefab : evenPrefab;
            if (!prefab) { Debug.LogError("SimpleLadderBuilder: Missing odd/even prefab."); return; }

            float x = isOdd ? oddX : evenX;
            float y = bottomPadding + (i - 1) * ySpacing;

            var go = Instantiate(prefab, content);
            go.name = $"Level_{i:00}";
            var rt = go.transform as RectTransform;
            rt.anchoredPosition = new Vector2(x, y);
            rt.localScale = Vector3.one;

            // number label (optional)
            var label = go.transform.Find(kNumber)?.GetComponent<TMP_Text>()
                      ?? go.GetComponentInChildren<TMP_Text>(true);
            if (label) label.text = i.ToString();

            // store the platform (circle) – if not found, use item center
            var plat = go.transform.Find(kPlatform) as RectTransform;
            platforms.Add(plat ? plat : rt);
        }

        // 4) Optional chest above the last level (opposite side)
        if (chestPrefab)
        {
            var chest = Instantiate(chestPrefab, content).transform as RectTransform;
            chest.name = "Chest";
            float lastY = bottomPadding + (totalLevels - 1) * ySpacing;
            bool lastIsOdd = (totalLevels % 2 == 1);
            float chestX = lastIsOdd ? evenX : oddX;
            chest.anchoredPosition = new Vector2(chestX, lastY + ySpacing * 0.6f);
        }

        // 5) Resize Content so everything fits
        float neededHeight = bottomPadding + (totalLevels - 1) * ySpacing + topPadding;
        var sz = content.sizeDelta;
        content.sizeDelta = new Vector2(sz.x, Mathf.Max(sz.y, neededHeight));

        // 6) Snap the scroll to the bottom so level 1 is visible
        if (snapToLevel1)
        {
            Canvas.ForceUpdateCanvases();
            scroll.verticalNormalizedPosition = 0f;  // bottom with bottom-pivot content
        }
    }

    void ClearGenerated()
    {
        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var t = content.GetChild(i);
            if (t.name.StartsWith("Level_") || t.name == "Chest")
            {
                if (Application.isPlaying) Destroy(t.gameObject);
                else DestroyImmediate(t.gameObject);
            }
        }
    }

    // Convenience for other scripts (1-based)
    public RectTransform GetPlatform(int level)
    {
        int idx = level - 1;
        return (idx >= 0 && idx < platforms.Count) ? platforms[idx] : null;
    }
}
