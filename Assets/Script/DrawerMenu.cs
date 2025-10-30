using System.Collections;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DrawerMenu : MonoBehaviour
{
    [Header("Wiring (required)")]
    public RectTransform drawer;   // sliding panel (pivot 1,0.5; anchors 1,0.5)
    public Image blocker;          // fullscreen black overlay (stretch both)

    [Header("Animation")]
    public float openDuration = 0.25f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool startClosed = true;

    [Header("Optional (only if you added them)")]
    public CanvasGroup profileDrawerGroup; // fade whole container
    public CanvasGroup drawerGroup;        // fade just the panel

    Vector2 openPos, closedPos;
    float drawerWidth = -1f;
    bool isOpen;
    bool inited;
    Coroutine anim;

    void Awake()
    {
        if (!drawer || !blocker)
        {
            Debug.LogError("[DrawerMenu] Assign 'drawer' and 'blocker'.");
            enabled = false; return;
        }

        // Ensure Blocker has a CanvasGroup + Button
        if (!blocker.TryGetComponent(out CanvasGroup _))
            blocker.gameObject.AddComponent<CanvasGroup>();
        var btn = blocker.GetComponent<Button>();
        if (!btn) btn = blocker.gameObject.AddComponent<Button>();
        btn.onClick.RemoveAllListeners();
        btn.onClick.AddListener(Close);

        // We initialize in Start (after first layout pass)
    }

    IEnumerator Start()
    {
        // Wait one frame to let Canvas Scaler & Layout complete
        yield return null;
        RecalculatePositions();
        SetInstant(!startClosed);
        inited = true;
    }

    void RecalculatePositions()
    {
        // Read the actual width *now*
        drawerWidth = drawer.rect.width;
        if (drawerWidth <= 0f)
        {
            // Fallback to sizeDelta if needed
            drawerWidth = Mathf.Abs(drawer.sizeDelta.x);
        }
        openPos = Vector2.zero;
        closedPos = new Vector2(drawerWidth, 0f);
        // Debug.Log($"[DrawerMenu] width={drawerWidth}, open={openPos}, closed={closedPos}");
    }

    void OnRectTransformDimensionsChange()
    {
        // If orientation or resolution changes, keep things correct
        if (!drawer) return;
        RecalculatePositions();
        if (!isOpen) drawer.anchoredPosition = closedPos;
    }

    void SetInstant(bool open)
    {
        isOpen = open;
        drawer.anchoredPosition = open ? openPos : closedPos;

        var bcg = blocker.GetComponent<CanvasGroup>();
        bcg.alpha = open ? 1f : 0f;
        bcg.blocksRaycasts = open;
        bcg.interactable = open;

        if (profileDrawerGroup) profileDrawerGroup.alpha = open ? 1f : 0f;
        if (drawerGroup) drawerGroup.alpha = open ? 1f : 0f;
    }

    IEnumerator Animate(bool open)
    {
        if (!inited) { RecalculatePositions(); SetInstant(!startClosed); inited = true; }

        isOpen = open;

        var bcg = blocker.GetComponent<CanvasGroup>();
        bcg.blocksRaycasts = true; // catch taps while animating
        bcg.interactable = true;

        float t = 0f, dur = Mathf.Max(0.01f, openDuration);

        Vector2 from = drawer.anchoredPosition;
        Vector2 to = open ? openPos : closedPos;

        float bFrom = bcg.alpha, bTo = open ? 1f : 0f;
        float pdFrom = profileDrawerGroup ? profileDrawerGroup.alpha : 1f;
        float pdTo = open ? 1f : 0f;
        float dFrom = drawerGroup ? drawerGroup.alpha : 1f;
        float dTo = open ? 1f : 0f;

        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float u = ease.Evaluate(t / dur);

            drawer.anchoredPosition = Vector2.LerpUnclamped(from, to, u);
            bcg.alpha = Mathf.LerpUnclamped(bFrom, bTo, u);
            if (profileDrawerGroup) profileDrawerGroup.alpha = Mathf.LerpUnclamped(pdFrom, pdTo, u);
            if (drawerGroup) drawerGroup.alpha = Mathf.LerpUnclamped(dFrom, dTo, u);
            yield return null;
        }

        drawer.anchoredPosition = to;
        bcg.alpha = bTo;
        if (!open) { bcg.blocksRaycasts = false; bcg.interactable = false; }
        anim = null;
    }

    public void Open()
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Animate(true));
    }
    public void Close()
    {
        if (anim != null) StopCoroutine(anim);
        anim = StartCoroutine(Animate(false));
    }
    public void Toggle() { if (isOpen) Close(); else Open(); }

    void Update()
    {
        if (isOpen && Input.GetKeyDown(KeyCode.Escape)) Close();
    }
}
