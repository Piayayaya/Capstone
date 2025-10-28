using UnityEngine;
using UnityEngine.EventSystems;

[RequireComponent(typeof(RectTransform))]
public class UIDropSlot : MonoBehaviour, IDropHandler
{
    public RectTransform snapPoint;      // optional
    [HideInInspector] public UIDragLetter current;
    [HideInInspector] public DragDropGameController controller;

    void Reset()
    {
        snapPoint = GetComponent<RectTransform>();
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (!UIDragLetter.GlobalAllowDrag) return;

        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<UIDragLetter>() : null;
        if (!drag) return;

        // free the old slot that was holding this letter
        if (drag.currentSlot && drag.currentSlot != this)
            drag.currentSlot.current = null;

        // occupy this slot with the dragged letter
        current = drag;
        drag.currentSlot = this;

        var target = snapPoint ? snapPoint : (RectTransform)transform;
        drag.transform.SetParent(target, false);

        var rt = drag.GetComponent<RectTransform>();
        rt.anchorMin = rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.localRotation = Quaternion.identity;
        rt.localScale = Vector3.one;
        drag.transform.SetAsLastSibling();

        var cg = drag.GetComponent<CanvasGroup>();
        if (cg) { cg.blocksRaycasts = true; cg.alpha = 1f; }

        controller?.OnSlotFilled(this);
    }
}
