using System.Collections;
using TMPro;
using UnityEngine;

public class CoinPopup : MonoBehaviour
{
    [Header("Refs")]
    public CanvasGroup group;               // CanvasGroup on CoinPopUp (parent)
    public TextMeshProUGUI amountText;      // CoinCanvas/Amount_Text

    [Header("Timing")]
    public float fade = 0.2f;
    public float stay = 1.1f;

    void Reset()
    {
        group = GetComponent<CanvasGroup>();
        if (group)
        {
            group.alpha = 0;
            group.interactable = false;
            group.blocksRaycasts = false;
        }
    }

    public void Show(int award)
    {
        if (!group || !amountText) return;

        amountText.enableAutoSizing = true;
        amountText.fontSizeMin = 18;
        amountText.fontSizeMax = 120;
        amountText.alignment = TextAlignmentOptions.Center;
        amountText.color = Color.black;              // make sure it’s visible
        amountText.text = award >= 10 ? "+10 COINS"
                           : award >= 5 ? "+5 COINS"
                           : "+3 COINS";
        amountText.ForceMeshUpdate();

        StopAllCoroutines();
        StartCoroutine(ShowRoutine());
    }

    IEnumerator ShowRoutine()
    {
        group.interactable = false;
        group.blocksRaycasts = false;

        // Fade in
        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / fade)
        {
            group.alpha = Mathf.Lerp(0, 1, t);
            yield return null;
        }
        group.alpha = 1;

        yield return new WaitForSecondsRealtime(stay);

        // Fade out
        for (float t = 0; t < 1f; t += Time.unscaledDeltaTime / fade)
        {
            group.alpha = Mathf.Lerp(1, 0, t);
            yield return null;
        }
        group.alpha = 0;
    }
}
