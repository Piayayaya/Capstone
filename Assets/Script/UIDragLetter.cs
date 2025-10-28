using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(CanvasGroup))]
public class UIDragLetter : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public char Letter = 'A';

    // gating from controller
    public static bool GlobalAllowDrag = true;

    CanvasGroup cg;
    RectTransform rt;
    Transform homeParent;
    Vector2 homeAnchoredPos;

    [HideInInspector] public UIDropSlot currentSlot;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
        cg = GetComponent<CanvasGroup>();
        RememberHome();
    }

    public void RememberHome()
    {
        homeParent = transform.parent;
        homeAnchoredPos = rt.anchoredPosition;
    }

    public void SnapToHome()
    {
        currentSlot = null;
        transform.SetParent(homeParent, false);
        rt.anchoredPosition = homeAnchoredPos;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;

        if (cg) { cg.blocksRaycasts = true; cg.alpha = 1f; }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!GlobalAllowDrag) return;
        if (cg) { cg.blocksRaycasts = false; }
        transform.SetAsLastSibling();
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!GlobalAllowDrag) return;
        rt.anchoredPosition += eventData.delta / GetCanvasScale();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (cg) cg.blocksRaycasts = true;
        // if not snapped by a slot, go home
        if (transform.parent == homeParent)
            SnapToHome();
    }

    float GetCanvasScale()
    {
        var c = GetComponentInParent<Canvas>();
        return c ? c.scaleFactor : 1f;
    }
}
