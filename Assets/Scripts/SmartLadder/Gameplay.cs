using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using System.Collections;

public class Gameplay : MonoBehaviour
{
    [Header("Refs")]
    public Transform player;                 // under Content
    public Transform[] points;               // Level 1..N (in order, under Content)
    public GameObject questionPanel;         // panel to show

    [Header("Start Panel (optional)")]
    public GameObject startPanel;            // assign your StartPanel root

    [Header("Quiz Hook")]
    public SmartLadderQuiz quiz;             // usually on QuestionPanel
    public bool callQuizOnArrive = true;

    [Header("Movement")]
    [Range(0.1f, 2f)] public float moveDuration = 0.7f;
    public Vector3 landingOffset = Vector3.zero;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Auto Start")]
    public bool autoMoveToFirstOnStart = true;
    public int startLevel1Based = 1;

    [Header("Question Panel Behavior")]
    public bool hidePanelDuringMove = true;
    public bool autoShowOnArrive = true;
    public float arriveShowDelay = 0.5f;

    [Header("Scroll View Follow (optional)")]
    public ScrollRect scroll;                // Scroll View
    public RectTransform content;            // Viewport -> Content
    public bool centerOnPlayer = true;       // center viewport on arrival
    public bool smoothScroll = true;
    public float scrollDuration = 0.25f;

    [Header("Initial Scroll Position")]
    public bool setInitialScroll = true;     // snap once before anything
    [Range(0f, 1f)] public float initialNormalized = 0f; // 0 = bottom, 1 = top

    [Header("Events")]
    public UnityEvent<int> onArrived;

    public int CurrentIndex { get; private set; } = -1;

    bool isMoving = false;
    Coroutine pendingShow, pendingQuiz, pendingCenter;

    void Awake()
    {
        if (questionPanel) questionPanel.SetActive(false);
        // Ensure StartPanel is visible when entering scene (optional)
        if (startPanel) startPanel.SetActive(true);
    }

    IEnumerator Start()
    {
        if (!player) { Debug.LogError("Gameplay: Assign 'player'."); yield break; }
        if (points == null || points.Length == 0) { Debug.LogError("Gameplay: 'points' is empty."); yield break; }

        // Let Canvas/ScrollView finish layout
        yield return null;
        Canvas.ForceUpdateCanvases();

        // Start scrolled at bottom (or your chosen position)
        if (setInitialScroll && scroll)
        {
            scroll.velocity = Vector2.zero;
            scroll.verticalNormalizedPosition = Mathf.Clamp01(initialNormalized);
            Canvas.ForceUpdateCanvases();
        }

        // If a StartPanel is up, do NOT auto-move.
        bool blockAuto = startPanel && startPanel.activeSelf;
        if (autoMoveToFirstOnStart && !blockAuto)
            MoveToLevel(startLevel1Based);
        else if (centerOnPlayer)
            CenterViewportOnPlayer(instant: true);
    }

    // ----- Called by StartPanel’s Start Game button -----
    public void BeginGame()
    {
        if (startPanel) startPanel.SetActive(false);
        if (hidePanelDuringMove && questionPanel) questionPanel.SetActive(false);
        MoveToLevel(startLevel1Based);    // this will center & show question on arrive
    }

    // ---------- Public API ----------
    public void SnapToLevel(int level1Based)
    {
        int idx = Mathf.Clamp(level1Based - 1, 0, points.Length - 1);
        SnapToIndex(idx);
        HandleArrive(idx);
    }

    public void MoveToLevel(int level1Based)
    {
        int idx = Mathf.Clamp(level1Based - 1, 0, points.Length - 1);
        MoveToIndex(idx);
    }

    public void MoveNext()
    {
        if (CurrentIndex < 0) { MoveToIndex(0); return; }
        if (CurrentIndex + 1 < points.Length) MoveToIndex(CurrentIndex + 1);
    }

    public void MovePrev()
    {
        if (CurrentIndex > 0) MoveToIndex(CurrentIndex - 1);
    }

    // ---------- Internals ----------
    void SnapToIndex(int idx)
    {
        if (!IsValid(idx)) return;
        player.position = points[idx].position + landingOffset;
        CurrentIndex = idx;
    }

    void MoveToIndex(int idx)
    {
        if (!IsValid(idx) || isMoving) return;

        if (pendingShow != null) { StopCoroutine(pendingShow); pendingShow = null; }
        if (pendingQuiz != null) { StopCoroutine(pendingQuiz); pendingQuiz = null; }
        if (pendingCenter != null) { StopCoroutine(pendingCenter); pendingCenter = null; }

        if (hidePanelDuringMove && questionPanel) questionPanel.SetActive(false);

        Vector3 target = points[idx].position + landingOffset;

        if ((player.position - target).sqrMagnitude < 0.0001f)
        {
            SnapToIndex(idx);
            HandleArrive(idx);
            return;
        }

        StopAllCoroutines();
        StartCoroutine(CoMove(target, idx));
    }

    IEnumerator CoMove(Vector3 targetPos, int targetIndex)
    {
        isMoving = true;
        Vector3 start = player.position;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, moveDuration);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            player.position = Vector3.LerpUnclamped(start, targetPos, k);
            yield return null;
        }

        player.position = targetPos;
        CurrentIndex = targetIndex;
        isMoving = false;

        HandleArrive(targetIndex);
    }

    void HandleArrive(int idx)
    {
        if (centerOnPlayer) CenterViewportOnPlayer();

        if (autoShowOnArrive && questionPanel)
        {
            if (pendingShow != null) StopCoroutine(pendingShow);
            pendingShow = StartCoroutine(CoShowPanelDelayed(arriveShowDelay));
        }

        if (callQuizOnArrive && quiz != null)
        {
            float delay = (autoShowOnArrive ? arriveShowDelay : 0f);
            if (pendingQuiz != null) StopCoroutine(pendingQuiz);
            pendingQuiz = StartCoroutine(CoCallQuizAfter(delay));
        }

        onArrived?.Invoke(idx);
    }

    IEnumerator CoShowPanelDelayed(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        if (questionPanel) questionPanel.SetActive(true);
        pendingShow = null;
    }

    IEnumerator CoCallQuizAfter(float delay)
    {
        if (delay > 0f) yield return new WaitForSeconds(delay);
        quiz.ShowNextQuestion();
        pendingQuiz = null;
    }

    // ---------- Scroll centering ----------
    public void CenterViewportOnPlayer(bool instant = false)
    {
        if (!scroll || !content) return;

        if (pendingCenter != null) { StopCoroutine(pendingCenter); pendingCenter = null; }
        if (instant || !smoothScroll)
            SnapViewportTo(player.position);
        else
            pendingCenter = StartCoroutine(CoCenterTo(player.position));
    }

    void SnapViewportTo(Vector3 playerWorldPos)
    {
        var viewport = (RectTransform)scroll.viewport;
        if (!viewport) return;

        float targetY = content.InverseTransformPoint(playerWorldPos).y;
        Vector3[] vp = new Vector3[4];
        viewport.GetWorldCorners(vp);
        Vector3 vc = (vp[0] + vp[2]) * 0.5f;
        float viewY = content.InverseTransformPoint(vc).y;

        float delta = targetY - viewY;
        Vector2 pos = content.anchoredPosition;
        pos.y += delta;

        float span = Mathf.Max(0f, content.rect.height - viewport.rect.height);
        pos.y = Mathf.Clamp(pos.y, 0f, span);

        content.anchoredPosition = pos;
        scroll.velocity = Vector2.zero;
    }

    IEnumerator CoCenterTo(Vector3 playerWorldPos)
    {
        var viewport = (RectTransform)scroll.viewport;
        if (!viewport) yield break;

        float startY = content.anchoredPosition.y;

        float TargetContentY()
        {
            float targetY = content.InverseTransformPoint(playerWorldPos).y;
            Vector3[] vp = new Vector3[4];
            viewport.GetWorldCorners(vp);
            Vector3 vc = (vp[0] + vp[2]) * 0.5f;
            float viewY = content.InverseTransformPoint(vc).y;

            float delta = targetY - viewY;
            float desired = startY + delta;
            float span = Mathf.Max(0f, content.rect.height - viewport.rect.height);
            return Mathf.Clamp(desired, 0f, span);
        }

        float endY = TargetContentY();
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / Mathf.Max(0.01f, scrollDuration);
            float k = ease.Evaluate(Mathf.Clamp01(t));
            Vector2 pos = content.anchoredPosition;
            pos.y = Mathf.LerpUnclamped(startY, endY, k);
            content.anchoredPosition = pos;
            scroll.velocity = Vector2.zero;
            yield return null;
        }

        pendingCenter = null;
    }

    // ---------- Utils ----------
    bool IsValid(int idx)
    {
        if (points == null || points.Length == 0) return false;
        if (idx < 0 || idx >= points.Length) { Debug.LogWarning("Gameplay: index out of range."); return false; }
        if (points[idx] == null) { Debug.LogWarning($"Gameplay: points[{idx}] is null."); return false; }
        return true;
    }
}
