using UnityEngine;
using UnityEngine.EventSystems;

public class UIDropSlot : MonoBehaviour, IDropHandler
{
    [Header("Accepts")]
    public char acceptsLetter = 'B';     // set per-slot in the Inspector

    [Header("Snap")]
    public RectTransform snapPoint;      // optional (defaults to this rect)

    [HideInInspector] public UIDragLetter current;       // who is inside now
    [HideInInspector] public DragDropGameController controller;

    public bool IsCorrect =>
        current != null &&
        char.ToUpperInvariant(current.Letter) == char.ToUpperInvariant(acceptsLetter);

    public void OnDrop(PointerEventData eventData)
    {
        if (current != null) return; // already filled

        // get the dragged letter
        var drag = eventData.pointerDrag ? eventData.pointerDrag.GetComponent<UIDragLetter>() : null;
        if (drag == null) return;

        // reject wrong letter
        if (char.ToUpperInvariant(drag.Letter) != char.ToUpperInvariant(acceptsLetter))
        {
            drag.ReturnToStart();
            controller?.WrongDropFeedback();   // optional visual/audio feedback
            return;
        }

        // accept + snap (robust — avoids disappearing)
        current = drag;

        var target = snapPoint != null ? snapPoint : (RectTransform)transform;

        drag.transform.SetParent(target, worldPositionStays: false);

        var dragRect = drag.GetComponent<RectTransform>();
        dragRect.anchorMin = dragRect.anchorMax = new Vector2(0.5f, 0.5f);
        dragRect.pivot = new Vector2(0.5f, 0.5f);
        dragRect.anchoredPosition = Vector2.zero;
        dragRect.localRotation = Quaternion.identity;
        dragRect.localScale = Vector3.one;
        dragRect.SetAsLastSibling();

        var cg = drag.GetComponent<CanvasGroup>();
        if (cg) { cg.blocksRaycasts = true; cg.alpha = 1f; }

        controller?.OnSlotFilled(this);
    }
}
