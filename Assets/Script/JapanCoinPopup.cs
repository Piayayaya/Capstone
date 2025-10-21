using System.Collections;
using TMPro;
using UnityEngine;

public class JapanCoinPopup : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup group;              // JP_CoinPopUp (CanvasGroup on the root)
    public TextMeshProUGUI amountText;     // JP_AmountText

    [Header("Timing")]
    public float fade = 0.2f;
    public float stay = 1.1f;

    void Awake()
    {
        if (!group) group = GetComponent<CanvasGroup>();
        // make sure the amount text renders on top of the box & icon
        if (amountText) amountText.transform.SetAsLastSibling();
        HideImmediate();
    }

    void HideImmediate()
    {
        if (!group) return;
        group.alpha = 0f;
        group.interactable = false;
        group.blocksRaycasts = false;
    }

    public void Show(int award)
    {
        if (!group || !amountText) return;

        // Show exactly "+10", "+5", or "+3"
        string msg = award >= 10 ? "+10 COINS" : award >= 5 ? "+5 COINS" : "+3 COINS";
        amountText.text = msg;
        amountText.enabled = true;   // just in case it was disabled

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        group.interactable = false;
        group.blocksRaycasts = false;

        for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(0f, 1f, t / fade);
            yield return null;
        }
        group.alpha = 1f;

        yield return new WaitForSecondsRealtime(stay);

        for (float t = 0f; t < fade; t += Time.unscaledDeltaTime)
        {
            group.alpha = Mathf.Lerp(1f, 0f, t / fade);
            yield return null;
        }
        group.alpha = 0f;
    }
}
