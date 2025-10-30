using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class SeeItSimpleCoinPopup : MonoBehaviour
{
    [Header("Wire these in Inspector")]
    [SerializeField] RectTransform root;     // SeeItCoinPopup RectTransform
    [SerializeField] CanvasGroup group;      // CanvasGroup on SeeItCoinPopup
    [SerializeField] TMP_Text amountText;    // "Amount Coins" TMP
    [SerializeField] Image icon;             // "Icon" Image (optional)

    [Header("Text")]
    [SerializeField] string label = "COINS";
    [SerializeField] int defaultAmount = 10;

    [Header("Timing (seconds)")]
    [SerializeField] float fadeIn = 0.18f;
    [SerializeField] float hold = 0.90f;
    [SerializeField] float fadeOut = 0.25f;

    [Header("Pop scale")]
    [SerializeField] float startScale = 0.80f;
    [SerializeField] float popScale = 1.08f;

    [Header("Audio (optional)")]
    [SerializeField] AudioSource sfx;

    Coroutine _playing;

    void Awake()
    {
        // Auto-wire if left empty
        if (!root) root = GetComponent<RectTransform>();
        if (!group) group = GetComponent<CanvasGroup>();
        if (!amountText) amountText = GetComponentsInChildren<TMP_Text>(true)
                                        .FirstOrDefault(t => t.gameObject.name.Contains("Amount"));
        if (!icon) icon = GetComponentsInChildren<Image>(true)
                            .FirstOrDefault(i => i.gameObject.name.Contains("Icon"));

        HideImmediate();
    }

    public void HideImmediate()
    {
        if (group)
        {
            group.alpha = 0f;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
        if (root) root.localScale = Vector3.one * startScale;
    }

    public void ShowAward() => Show(defaultAmount);
    public void ShowAward(int amt) => Show(amt);

    void Show(int amount)
    {
        if (amountText) amountText.text = $"+{amount} {label}";
        if (_playing != null) StopCoroutine(_playing);
        _playing = StartCoroutine(Play());
    }

    System.Collections.IEnumerator Play()
    {
        if (sfx) sfx.Play();

        group.interactable = false;
        group.blocksRaycasts = false;

        // fade in + pop
        float t = 0f;
        if (root) root.localScale = Vector3.one * startScale;
        while (t < fadeIn)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeIn);
            group.alpha = k;
            if (root) root.localScale = Vector3.one * Mathf.Lerp(startScale, popScale, k);
            yield return null;
        }
        group.alpha = 1f;

        yield return new WaitForSecondsRealtime(hold);

        // fade out
        t = 0f;
        while (t < fadeOut)
        {
            t += Time.unscaledDeltaTime;
            float k = 1f - Mathf.Clamp01(t / fadeOut);
            group.alpha = k;
            yield return null;
        }

        HideImmediate();
        _playing = null;
    }
}
