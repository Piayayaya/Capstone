using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DrawerMenu : MonoBehaviour
{
    [Header("Wiring")]
    public RectTransform drawer;      // assign ProfileDrawer/Panel (the sliding white panel)
    public CanvasGroup drawerCg;      // CanvasGroup on the same object as 'drawer'
    public GameObject blocker;        // assign ProfileDrawer/Blocker (has Button + Image)

    [Header("Motion")]
    public float openX = 0f;          // opened X
    public float tweenSeconds = 0.25f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optional Callbacks")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    float _closedX;
    bool _isOpen;
    bool _isAnimating;

    void Awake()
    {
        if (!drawer)
        {
            Debug.LogError("[DrawerMenu] Drawer is not assigned!", this);
            return;
        }

        if (!drawerCg)
            drawerCg = drawer.GetComponent<CanvasGroup>();

        // compute closed position based on current width
        _closedX = -drawer.rect.width;

        // start closed
        CloseImmediate();
    }

    void OnEnable()
    {
        // ensure it draws on top of other UI
        transform.SetAsLastSibling();
    }

    // === called by profile button ===
    public void Toggle()
    {
        if (_isAnimating) return;
        if (_isOpen) Close();
        else Open();
    }

    public void Open()
    {
        if (_isAnimating || _isOpen) return;
        StopAllCoroutines();
        StartCoroutine(Slide(true));
    }

    public void Close()
    {
        if (_isAnimating || !_isOpen) return;
        StopAllCoroutines();
        StartCoroutine(Slide(false));
    }

    // === called by Blocker button ===
    public void OnBlockerClicked()
    {
        // so clicking the transparent area closes the drawer
        if (!_isAnimating && _isOpen)
            Close();
    }

    void CloseImmediate()
    {
        _isOpen = false;
        _isAnimating = false;

        Vector2 p = drawer.anchoredPosition;
        p.x = _closedX;
        drawer.anchoredPosition = p;

        if (drawerCg)
        {
            drawerCg.alpha = 0f;
            drawerCg.interactable = false;
            drawerCg.blocksRaycasts = false;
        }

        if (blocker)
            blocker.SetActive(false);
    }

    IEnumerator Slide(bool show)
    {
        _isAnimating = true;
        transform.SetAsLastSibling();               // keep on top

        if (blocker)
            blocker.SetActive(true);

        float startX = drawer.anchoredPosition.x;
        float endX = show ? openX : _closedX;

        if (drawerCg)
        {
            drawerCg.blocksRaycasts = true;
            drawerCg.interactable = false;         // until finished
        }

        float t = 0f;
        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / Mathf.Max(0.01f, tweenSeconds);
            float k = curve.Evaluate(Mathf.Clamp01(t));

            Vector2 p = drawer.anchoredPosition;
            p.x = Mathf.Lerp(startX, endX, k);
            drawer.anchoredPosition = p;

            if (drawerCg)
                drawerCg.alpha = Mathf.Lerp(show ? 0f : 1f, show ? 1f : 0f, k);

            yield return null;
        }

        _isOpen = show;
        _isAnimating = false;

        if (drawerCg)
        {
            drawerCg.interactable = show;
            drawerCg.blocksRaycasts = show;
        }

        if (blocker)
            blocker.SetActive(show);

        if (show) onOpened?.Invoke();
        else onClosed?.Invoke();

        // we keep drawer active always so layout stays fine
        drawer.gameObject.SetActive(true);
    }
}
