using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class DragDropGameController : MonoBehaviour
{
    // -------------------- Slots --------------------
    [Header("Slots (left → right in list)")]
    public List<UIDropSlot> slots = new();

    // -------------------- Owl cloud labels --------------------
    [Header("Owl bubble labels")]
    public TextMeshProUGUI bubbleText;        // Instruction/congrats label in the cloud
    [TextArea] public string startMessage = "COMPLETE THE EMPTY BOX\nBELOW!";
    [TextArea] public string congratsMessage = "WOW! CONGRATS! FOR\nCOMPLETING THE EMPTY\nBOX!";
    public TextMeshProUGUI bubbleTimerText;   // Timer label in the SAME cloud

    // -------------------- Coins --------------------
    [Header("Coins")]
    public CoinPopup_DragDrop coinPopup;
    public int rewardAmount = 10;

    // -------------------- Hint confirm --------------------
    [Header("Hint – Confirm (YES/NO)")]
    public CanvasGroup confirmPanel;
    public TextMeshProUGUI confirmText;
    [Min(0)] public int startingHints = 1;

    // -------------------- Center toast --------------------
    [Header("Center Toast")]
    public Toast toastPanel;

    // -------------------- Messages --------------------
    [Header("Messages")]
    [SerializeField] string confirmMsgFormat = "ARE YOU SURE TO USE THE HINT?\n\nHINTS LEFT: {0}";
    [SerializeField] string toastNoHints = "NO MORE HINTS LEFT";
    [SerializeField] string toastNothing = "NOTHING TO HINT!";
    [SerializeField] string toastUsedFormat = "HINT USED • LEFT: {0}";
    [SerializeField] string wrongAnswerMsg = "Not quite... try again!";
    [SerializeField] string timesUpBubbleMsg = "TIME'S UP!";

    // -------------------- Start overlay + timer --------------------
    [Header("Start Overlay + Round Timer")]
    public StartOverlay startOverlay;      // "Are you ready?" -> "Game starts!"
    public float totalTimeSeconds = 60f;   // 1 minute

    // -------------------- Gameplay lock --------------------
    [Header("Gameplay Gate")]
    public CanvasGroup gameplayGroup;      // covers draggables + slots
    public Button hintButton;              // optional

    // -------------------- Restart Panel --------------------
    [Header("Restart Panel (play again)")]
    public CanvasGroup restartPanel;                 // << assign CanvasGroup on RESTARTPANEL
    public TextMeshProUGUI restartQuestionText;
    public RectTransform restartPanelRect;
    [SerializeField] string restartQuestion = "DO YOU WANT TO PLAY AGAIN?";
    [SerializeField] string gamemodesSceneName = "Gamemodes";
    [SerializeField, Tooltip("Pause after TIME'S UP! shows in the timer before opening Play Again.")]
    float delayBeforePlayAgain = 1.0f;

    // -------------------- Character bounce --------------------
    [Header("Character Animation")]
    [Tooltip("Assign the CharacterBouncer on the boy so it bounces during play.")]
    public CharacterBouncer characterBouncer;

    // -------------------- Internal state --------------------
    static readonly char[] Expected = new[] { 'B', 'O', 'Y' };

    int hintsLeft;
    bool awardedOnce;

    float timeLeft;
    bool timerRunning;

    enum ConfirmMode { None, Hint }
    ConfirmMode confirmMode = ConfirmMode.None;

    void Start()
    {
        hintsLeft = Mathf.Max(0, startingHints);
        awardedOnce = false;

        if (bubbleText)
        {
            bubbleText.gameObject.SetActive(true);
            bubbleText.text = startMessage;
        }

        if (bubbleTimerText) bubbleTimerText.gameObject.SetActive(false);

        foreach (var s in slots) if (s) s.controller = this;

        HideConfirm();
        RestartPanelInit();   // ensure panel is active+hidden at start

        SetGameplayLocked(true);
        StartCoroutine(BeginAfterSmallDelay());
    }

    void RestartPanelInit()
    {
        if (!restartPanel) return;
        // RESTARTPANEL GameObject should stay active so we can toggle via CanvasGroup
        if (!restartPanel.gameObject.activeSelf)
            restartPanel.gameObject.SetActive(true);

        restartPanel.alpha = 0f;
        restartPanel.interactable = false;
        restartPanel.blocksRaycasts = false;
    }

    IEnumerator BeginAfterSmallDelay()
    {
        yield return new WaitForSecondsRealtime(1.0f);

        if (startOverlay != null)
            yield return startOverlay.ShowSequenceAndHide("ARE YOU READY?", "GAME STARTS!");

        // Start timer
        timeLeft = totalTimeSeconds;

        if (bubbleTimerText)
        {
            if (bubbleText) bubbleText.gameObject.SetActive(false);
            bubbleTimerText.alignment = TextAlignmentOptions.Center;
            bubbleTimerText.enableAutoSizing = true;
            bubbleTimerText.gameObject.SetActive(true);
            UpdateTimerLabel();
        }

        timerRunning = true;
        SetGameplayLocked(false);

        // >>> START bounce while playing
        characterBouncer?.Play();
    }

    void Update()
    {
        if (!timerRunning) return;

        timeLeft -= Time.deltaTime;

        if (timeLeft <= 0f)
        {
            timeLeft = 0f;
            timerRunning = false;

            if (bubbleTimerText)
            {
                bubbleTimerText.gameObject.SetActive(true);
                bubbleTimerText.alignment = TextAlignmentOptions.Center;
                bubbleTimerText.enableAutoSizing = true;
                bubbleTimerText.text = timesUpBubbleMsg;
                bubbleTimerText.ForceMeshUpdate();
            }

            SetGameplayLocked(true);

            // >>> STOP bounce when time is up
            characterBouncer?.Stop();

            StartCoroutine(ShowRestartAfterDelay());   // TIME'S UP path
            return; // prevent overwriting with 0:00
        }

        UpdateTimerLabel();
    }

    void UpdateTimerLabel()
    {
        if (!bubbleTimerText) return;
        int sec = Mathf.CeilToInt(timeLeft);
        int m = sec / 60;
        int s = sec % 60;
        bubbleTimerText.text = $"{m:0}:{s:00}";
    }

    IEnumerator ShowRestartAfterDelay()
    {
        yield return new WaitForSecondsRealtime(Mathf.Max(0f, delayBeforePlayAgain));
        ShowRestart();
    }

    // >>> NEW: wait for coin popup to finish, then show restart (success path)
    IEnumerator ShowRestartAfterCoins()
    {
        // If no popup assigned, just use the same delay as time's up
        float wait = delayBeforePlayAgain;

        if (coinPopup)
        {
            // total time of the coin popup animation in your CoinPopup_DragDrop
            wait = coinPopup.appearDuration + coinPopup.holdTime + coinPopup.fadeOutDuration;
        }

        yield return new WaitForSecondsRealtime(wait);
        ShowRestart();
    }

    // -------------------- Play-again confirm --------------------
    void ShowRestart()
    {
        if (restartPanel != null)
        {
            if (restartQuestionText) restartQuestionText.text = restartQuestion;

            if (restartPanelRect != null)
            {
                restartPanelRect.anchorMin = restartPanelRect.anchorMax = new Vector2(0.5f, 0.5f);
                restartPanelRect.pivot = new Vector2(0.5f, 0.5f);
                restartPanelRect.anchoredPosition = Vector2.zero;
            }

            restartPanel.alpha = 1f;
            restartPanel.interactable = true;
            restartPanel.blocksRaycasts = true;
            restartPanel.gameObject.SetActive(true);
        }
        else
        {
            if (confirmText) confirmText.text = restartQuestion;
            confirmMode = ConfirmMode.None;
            ShowConfirm();
        }
    }

    void HideRestart()
    {
        if (restartPanel == null) return;
        restartPanel.alpha = 0f;
        restartPanel.interactable = false;
        restartPanel.blocksRaycasts = false;
        restartPanel.gameObject.SetActive(true); // keep active; we control via CanvasGroup
    }

    public void RestartYes()
    {
        HideRestart();
        var active = SceneManager.GetActiveScene().name;
        SceneManager.LoadScene(active);
    }

    public void RestartNo()
    {
        HideRestart();
        if (!string.IsNullOrEmpty(gamemodesSceneName))
            SceneManager.LoadScene(gamemodesSceneName);
    }

    // -------------------- Slot evaluation --------------------
    public void OnSlotFilled(UIDropSlot _)
    {
        if (!AllFilled()) return;

        if (IsCorrectInOrder())
        {
            if (!awardedOnce)
            {
                if (bubbleTimerText) bubbleTimerText.gameObject.SetActive(false);

                if (bubbleText)
                {
                    bubbleText.gameObject.SetActive(true);
                    bubbleText.text = congratsMessage;
                }

                CoinWallet_DragDrop.Add(rewardAmount);
                coinPopup?.Show(rewardAmount);
                awardedOnce = true;

                // lock gameplay + stop bounce
                SetGameplayLocked(true);
                characterBouncer?.Stop();
                // stop timer so it freezes at current text
                timerRunning = false;

                // >>> show restart AFTER coins finish animating
                StartCoroutine(ShowRestartAfterCoins());
            }
        }
        else
        {
            ShowCenter(wrongAnswerMsg);
            StartCoroutine(ResetLettersWhenToastDone());
        }
    }

    public void WrongDropFeedback() => ShowCenter(wrongAnswerMsg);

    // -------------------- Hints --------------------
    public void OnHintButton()
    {
        if (hintsLeft <= 0) { ShowCenter(toastNoHints); return; }
        if (!HasEmptySlot()) { ShowCenter(toastNothing); return; }

        confirmMode = ConfirmMode.Hint;
        if (confirmText) confirmText.text = string.Format(confirmMsgFormat, hintsLeft);
        ShowConfirm();
    }

    public void ConfirmHintYes()
    {
        if (confirmMode == ConfirmMode.Hint)
        {
            HideConfirm();
            UseHint();
        }
        else
        {
            HideConfirm();
            RestartYes();
        }
        confirmMode = ConfirmMode.None;
    }

    public void ConfirmHintNo()
    {
        if (confirmMode == ConfirmMode.Hint)
        {
            HideConfirm();
        }
        else
        {
            HideConfirm();
            RestartNo();
        }
        confirmMode = ConfirmMode.None;
    }

    void ShowConfirm()
    {
        if (!confirmPanel) return;
        confirmPanel.gameObject.SetActive(true);
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

    // -------------------- Utility --------------------
    void SetGameplayLocked(bool locked)
    {
        if (gameplayGroup)
        {
            gameplayGroup.interactable = !locked;
            gameplayGroup.blocksRaycasts = !locked;
        }
        if (hintButton) hintButton.interactable = !locked;
    }

    void ShowCenter(string msg)
    {
        if (toastPanel != null) toastPanel.Show(msg);
        else Debug.LogWarning("Assign CenterToast (Toast) to DragDropGameController → Toast Panel.");
    }

    bool AllFilled()
    {
        foreach (var s in slots)
            if (s == null || s.current == null) return false;
        return true;
    }

    bool IsCorrectInOrder()
    {
        if (slots == null || slots.Count < Expected.Length) return false;
        for (int i = 0; i < Expected.Length; i++)
        {
            var s = slots[i];
            if (s == null || s.current == null) return false;
            if (char.ToUpperInvariant(s.current.Letter) != Expected[i]) return false;
        }
        return true;
    }

    IEnumerator ResetLettersWhenToastDone()
    {
        float wait = 0.35f;
        if (toastPanel != null) wait = toastPanel.TotalDuration;
        yield return new WaitForSecondsRealtime(wait);

        foreach (var s in slots) if (s) s.current = null;

        var letters = GetAll<UIDragLetter>();
        foreach (var l in letters) l.SnapToHome();
    }

    bool HasEmptySlot()
    {
        foreach (var s in slots)
            if (s != null && s.current == null) return true;
        return false;
    }

    void UseHint()
    {
        if (hintsLeft <= 0) { ShowCenter(toastNoHints); return; }

        // First empty slot (left to right)
        UIDropSlot targetSlot = null;
        for (int i = 0; i < slots.Count; i++)
        {
            if (slots[i] != null && slots[i].current == null) { targetSlot = slots[i]; break; }
        }
        if (targetSlot == null) { ShowCenter(toastNothing); return; }

        int slotIndex = slots.IndexOf(targetSlot);
        char neededForThisSlot = Expected[slotIndex];

        // If the correct letter is currently in a different slot, free it
        for (int i = 0; i < slots.Count; i++)
        {
            var s = slots[i];
            if (s != null && s.current != null && char.ToUpperInvariant(s.current.Letter) == neededForThisSlot && s != targetSlot)
            {
                s.current.SnapToHome();
                s.current = null;
                break;
            }
        }

        var letter = FindFreeLetter(neededForThisSlot) ?? FindAnyFreeLetter();
        if (letter == null) { ShowCenter("No free letter available"); return; }

        PlaceLetterInSlot(letter, targetSlot);

        hintsLeft = Mathf.Max(0, hintsLeft - 1);
        ShowCenter(hintsLeft <= 0 ? toastNoHints : string.Format(toastUsedFormat, hintsLeft));

        OnSlotFilled(targetSlot);
    }

    UIDragLetter FindFreeLetter(char needed)
    {
        var all = GetAll<UIDragLetter>();
        foreach (var l in all)
            if (char.ToUpperInvariant(l.Letter) == char.ToUpperInvariant(needed) && !IsInAnySlot(l)) return l;
        return null;
    }

    UIDragLetter FindAnyFreeLetter()
    {
        var all = GetAll<UIDragLetter>();
        foreach (var l in all) if (!IsInAnySlot(l)) return l;
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
        foreach (var s in slots)
            if (s != null && s.current == drag) s.current = null;

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
