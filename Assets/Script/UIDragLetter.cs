using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UIDragLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Config")]
    public string letterId = "B";   // Set per letter in Inspector (e.g., "B", "O", "Y")
    public Canvas canvas;           // Assign the root Canvas (the one that renders the UI)

    // Cached
    RectTransform rect;
    CanvasGroup group;

    // Start state
    Vector2 startPos;
    Transform startParent;

    public char Letter => string.IsNullOrEmpty(letterId) ? '?' : letterId[0];

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        group = GetComponent<CanvasGroup>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData e)
    {
        startPos = rect.anchoredPosition;
        startParent = rect.parent;

        group.blocksRaycasts = false;   // let slots receive raycasts
        group.alpha = 0.9f;
    }

    public void OnDrag(PointerEventData e)
    {
        rect.anchoredPosition += e.delta / (canvas ? canvas.scaleFactor : 1f);
    }

    public void OnEndDrag(PointerEventData e)
    {
        // If no slot adopted us, snap back
        if (rect.parent == startParent)
            rect.anchoredPosition = startPos;

        group.blocksRaycasts = true;
        group.alpha = 1f;
    }

    public void ReturnToStart()
    {
        rect.SetParent(startParent as RectTransform, false);
        rect.anchoredPosition = startPos;
        group.blocksRaycasts = true;
        group.alpha = 1f;
    }
}
