using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class DrawerMenu : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("Sliding white panel object (ProfileDrawer/Panel)")]
    public RectTransform drawer;          // assign ProfileDrawer/Panel
    [Tooltip("CanvasGroup on the same object as 'drawer'")]
    public CanvasGroup drawerCg;          // assign CanvasGroup on ProfileDrawer
    [Tooltip("Full-screen transparent button behind the drawer (ProfileDrawer/Blocker)")]
    public GameObject blocker;            // assign ProfileDrawer/Blocker

    [Header("Motion")]
    [Tooltip("X position when opened (usually 0).")]
    public float openX = 0f;
    [Tooltip("How long the slide animation takes.")]
    public float tweenSeconds = 0.25f;
    public AnimationCurve curve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optional Callbacks")]
    public UnityEvent onOpened;
    public UnityEvent onClosed;

    private float _closedX;
    private bool _isOpen;
    private bool _isAnimating;

    private void Awake()
    {
        if (!drawer)
        {
            Debug.LogError("[DrawerMenu] Drawer is not assigned!", this);
            return;
        }

        if (!drawerCg)
            drawerCg = drawer.GetComponent<CanvasGroup>();

        // Closed X based on current width
        _closedX = -drawer.rect.width;

        // Start closed
        CloseImmediate();
    }

    private void OnEnable()
    {
        // Make sure drawer UI is on top of other UI
        transform.SetAsLastSibling();
    }

    // === Hook this to your PROFILE BUTTON ===
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

    // === Hook this to the Blocker Button (ProfileDrawer/Blocker) ===
    public void OnBlockerClicked()
    {
        if (!_isAnimating && _isOpen)
            Close();
    }

    private void CloseImmediate()
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

    private IEnumerator Slide(bool show)
    {
        _isAnimating = true;
        transform.SetAsLastSibling();

        if (blocker)
            blocker.SetActive(true);

        float startX = drawer.anchoredPosition.x;
        float endX = show ? openX : _closedX;

        if (drawerCg)
        {
            drawerCg.blocksRaycasts = true;
            drawerCg.interactable = false;   // re-enabled at the end
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

        drawer.gameObject.SetActive(true);
    }
}
