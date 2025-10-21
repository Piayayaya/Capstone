using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DragDropGameController : MonoBehaviour
{
    [Header("Slots (left → right)")]
    public List<UIDropSlot> slots = new();

    [Header("Owl bubble text")]
    public TextMeshProUGUI bubbleText;
    [TextArea] public string startMessage = "COMPLETE THE EMPTY BOX BELOW!";
    [TextArea] public string congratsMessage = "WOW! CONGRATS! FOR\nCOMPLETING THE EMPTY\nBOX!";

    [Header("Coins")]
    public CoinPopup_DragDrop coinPopup;
    public int rewardAmount = 10;

    // ---------- HINT UI ----------
    [Header("Hint – Confirm (YES/NO)")]
    [Tooltip("The PARENT object that has the CanvasGroup (your HintConfirmPanel).")]
    public CanvasGroup confirmPanel;         // <- assign: HintConfirmPanel (parent)
    [Tooltip("The big message inside the confirm panel.")]
    public TextMeshProUGUI confirmText;      // <- assign: ConfirmText (child TMP)

    [Header("Hint – Toast (center)")]
    [Tooltip("The small center box for short messages (may have background + TMP).")]
    public CanvasGroup toastPanel;           // <- a centered object with CanvasGroup
    public TextMeshProUGUI toastText;        // <- TMP inside toastPanel

    [Min(0)] public int startingHints = 1;

    [Header("Hint Messages")]
    [SerializeField] string confirmMsgFormat = "ARE YOU SURE TO USE THE HINT?\n\nHINTS LEFT: {0}";
    [SerializeField] string toastNoHints = "NO MORE HINTS LEFT";
    [SerializeField] string toastNothing = "NOTHING TO HINT!";
    [SerializeField] string toastUsedFormat = "HINT USED • LEFT: {0}";
    [SerializeField, Min(0.1f)] float toastDuration = 1.6f;

    int hintsLeft;
    Coroutine toastCo;

    void Start()
    {
        hintsLeft = Mathf.Max(0, startingHints);

        if (bubbleText) bubbleText.text = startMessage;
        foreach (var s in slots) if (s) s.controller = this;

        // start hidden
        HideConfirm();
        if (toastPanel) { toastPanel.alpha = 0f; toastPanel.gameObject.SetActive(true); }
    }

    public void OnSlotFilled(UIDropSlot _)
    {
        if (!AllCorrect()) return;

        if (bubbleText) bubbleText.text = congratsMessage;

        if (coinPopup)
        {
            CoinWallet_DragDrop.Add(rewardAmount);
            coinPopup.Show(rewardAmount);
        }
    }

    public void WrongDropFeedback() { }

    bool AllCorrect()
    {
        foreach (var s in slots)
            if (s == null || !s.IsCorrect) return false;
        return true;
    }

    // ======================= HINT FLOW =======================
    public void OnHintButton()
    {
        if (hintsLeft <= 0) { ShowToast(toastNoHints); return; }
        if (!HasEmptySlot()) { ShowToast(toastNothing); return; }

        if (confirmText) confirmText.text = string.Format(confirmMsgFormat, hintsLeft);
        ShowConfirm();
    }

    public void ConfirmHintYes()
    {
        HideConfirm();
        UseHint();
    }

    public void ConfirmHintNo() => HideConfirm();

    void ShowConfirm()
    {
        if (!confirmPanel) return;
        confirmPanel.gameObject.SetActive(true);   // must be the PARENT panel
        confirmPanel.alpha = 1f;
        confirmPanel.interactable = true;
        confirmPanel.blocksRaycasts = true;
    }

    void HideConfirm()
    {
        if (!confirmPanel) return;
        confirmPanel.alpha = 0f;
        confirmPanel.interactable = false;
        confirmPanel.blocksRaycasts = false;
        confirmPanel.gameObject.SetActive(false);
    }

    bool HasEmptySlot()
    {
        foreach (var s in slots)
            if (s != null && s.current == null) return true;
        return false;
    }

    void UseHint()
    {
        if (hintsLeft <= 0) { ShowToast(toastNoHints); return; }

        UIDropSlot targetSlot = null;
        foreach (var s in slots) { if (s != null && s.current == null) { targetSlot = s; break; } }
        if (targetSlot == null) { ShowToast(toastNothing); return; }

        UIDragLetter freeLetter = FindFreeLetter(targetSlot.acceptsLetter);
        if (freeLetter == null) { ShowToast("NO MATCHING LETTER AVAILABLE"); return; }

        PlaceLetterInSlot(freeLetter, targetSlot);

        hintsLeft = Mathf.Max(0, hintsLeft - 1);
        ShowToast(hintsLeft <= 0 ? toastNoHints : string.Format(toastUsedFormat, hintsLeft));

        OnSlotFilled(targetSlot);
    }

    // ======================= TOAST =======================
    void ShowToast(string msg)
    {
        if (!toastPanel || !toastText) return;

        toastText.text = msg;
        if (toastCo != null) StopCoroutine(toastCo);
        toastCo = StartCoroutine(ToastRoutine());
    }

    IEnumerator ToastRoutine()
    {
        // fade in
        float t = 0f;
        toastPanel.gameObject.SetActive(true);
        while (t < 0.2f)
        {
            t += Time.unscaledDeltaTime;
            toastPanel.alpha = Mathf.Clamp01(t / 0.2f);
            yield return null;
        }

        yield return new WaitForSecondsRealtime(toastDuration);

        // fade out
        t = 0f;
        while (t < 0.25f)
        {
            t += Time.unscaledDeltaTime;
            toastPanel.alpha = 1f - Mathf.Clamp01(t / 0.25f);
            yield return null;
        }
        toastPanel.alpha = 0f;
        toastPanel.gameObject.SetActive(false);
    }

    // ---------------- helpers ----------------
    UIDragLetter FindFreeLetter(char needed)
    {
        var letters = GetAll<UIDragLetter>();
        foreach (var l in letters)
        {
            if (char.ToUpperInvariant(l.Letter) != char.ToUpperInvariant(needed)) continue;
            if (!IsInAnySlot(l)) return l;
        }
        return null;
    }

    bool IsInAnySlot(UIDragLetter letter)
    {
        foreach (var s in slots)
            if (s != null && s.current == letter) return true;
        return false;
    }

    void PlaceLetterInSlot(UIDragLetter drag, UIDropSlot slot)
    {
        slot.current = drag;
        var target = slot.snapPoint != null ? slot.snapPoint : (RectTransform)slot.transform;

        drag.transform.SetParent(target, false);
        var r = drag.GetComponent<RectTransform>();
        r.anchorMin = r.anchorMax = new Vector2(0.5f, 0.5f);
        r.pivot = new Vector2(0.5f, 0.5f);
        r.anchoredPosition = Vector2.zero;
        r.localRotation = Quaternion.identity;
        r.localScale = Vector3.one;
        r.SetAsLastSibling();

        var cg = drag.GetComponent<CanvasGroup>();
        if (cg) { cg.blocksRaycasts = true; cg.alpha = 1f; }
    }

    static T[] GetAll<T>() where T : Object
    {
#if UNITY_2023_1_OR_NEWER
        return Object.FindObjectsByType<T>(FindObjectsSortMode.None);
#else
        return Object.FindObjectsOfType<T>(true);
#endif
    }
}
