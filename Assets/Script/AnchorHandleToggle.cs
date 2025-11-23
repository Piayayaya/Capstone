using UnityEngine;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(Toggle))]
public class AnchorHandleToggle : MonoBehaviour
{
    [Header("Refs")]
    public Toggle toggle;                 // your Toggle component
    public RectTransform whiteHandle;     // the white circle/knob
    public RectTransform onAnchor;        // right position
    public RectTransform offAnchor;       // left position

    [Header("Optional")]
    public bool animate = true;
    public float animDuration = 0.12f;

    Coroutine moveRoutine;

    void Reset()
    {
        toggle = GetComponent<Toggle>();
    }

    void Awake()
    {
        if (toggle == null) toggle = GetComponent<Toggle>();
    }

    void OnEnable()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);

        // Force correct position at start
        SnapTo(toggle.isOn);
    }

    void OnDisable()
    {
        toggle.onValueChanged.RemoveListener(OnToggleChanged);
    }

    void OnToggleChanged(bool isOn)
    {
        if (!whiteHandle || !onAnchor || !offAnchor) return;

        if (animate)
        {
            if (moveRoutine != null) StopCoroutine(moveRoutine);
            moveRoutine = StartCoroutine(MoveHandle(isOn));
        }
        else
        {
            SnapTo(isOn);
        }
    }

    void SnapTo(bool isOn)
    {
        RectTransform target = isOn ? onAnchor : offAnchor;

        // If same parent, use anchoredPosition
        if (whiteHandle.parent == target.parent)
            whiteHandle.anchoredPosition = target.anchoredPosition;
        else
            whiteHandle.position = target.position;
    }

    IEnumerator MoveHandle(bool isOn)
    {
        RectTransform target = isOn ? onAnchor : offAnchor;

        Vector2 startPos;
        Vector2 endPos;

        if (whiteHandle.parent == target.parent)
        {
            startPos = whiteHandle.anchoredPosition;
            endPos = target.anchoredPosition;
        }
        else
        {
            startPos = whiteHandle.position;
            endPos = target.position;
        }

        float t = 0f;
        while (t < animDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / animDuration);

            Vector2 p = Vector2.Lerp(startPos, endPos, k);

            if (whiteHandle.parent == target.parent)
                whiteHandle.anchoredPosition = p;
            else
                whiteHandle.position = p;

            yield return null;
        }

        SnapTo(isOn);
        moveRoutine = null;
    }
}
