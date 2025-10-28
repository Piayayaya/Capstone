using System.Collections;
using TMPro;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class StartOverlay : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] CanvasGroup group;
    [SerializeField] TextMeshProUGUI textLabel;

    [Header("Timings")]
    [SerializeField] float fade = 0.15f;
    [SerializeField] float holdEach = 0.9f;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        textLabel = GetComponentInChildren<TextMeshProUGUI>(true);
    }

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
        gameObject.SetActive(true); // stays enabled; we just fade it
    }

    public IEnumerator ShowSequenceAndHide(string msg1, string msg2)
    {
        yield return ShowOne(msg1);
        yield return ShowOne(msg2);

        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    IEnumerator ShowOne(string msg)
    {
        if (!group || !textLabel) yield break;

        textLabel.text = msg;
        textLabel.ForceMeshUpdate();

        group.blocksRaycasts = true;
        group.interactable = true;

        float t = 0f;
        while (t < fade) { t += Time.unscaledDeltaTime; group.alpha = Mathf.Lerp(0f, 1f, t / fade); yield return null; }
        group.alpha = 1f;

        yield return new WaitForSecondsRealtime(holdEach);

        t = 0f;
        while (t < fade) { t += Time.unscaledDeltaTime; group.alpha = Mathf.Lerp(1f, 0f, t / fade); yield return null; }
        group.alpha = 0f;

        group.blocksRaycasts = false;
        group.interactable = false;
    }
}
