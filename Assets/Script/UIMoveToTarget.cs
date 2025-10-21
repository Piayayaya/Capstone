using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UIMoveToTarget : MonoBehaviour
{
    [Header("Who moves & where")]
    public RectTransform character;
    public RectTransform target;

    [Header("Motion")]
    public float speed = 700f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Events")]
    public UnityEvent onArrived;

    bool isMoving;

    public void StartMove()
    {
        if (isMoving || character == null || target == null) return;
        StartCoroutine(MoveRoutine());
    }

    IEnumerator MoveRoutine()
    {
        isMoving = true;

        Vector2 start = character.anchoredPosition;
        Vector2 end = target.anchoredPosition;

        float distance = Vector2.Distance(start, end);
        float dur = Mathf.Max(0.01f, distance / speed);
        float t = 0f;

        while (t < 1f)
        {
            t += Time.unscaledDeltaTime / dur;
            float k = ease.Evaluate(Mathf.Clamp01(t));
            character.anchoredPosition = Vector2.LerpUnclamped(start, end, k);
            yield return null;
        }

        character.anchoredPosition = end;
        isMoving = false;
        onArrived?.Invoke();
    }
}
