using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class NameTheFlagController : MonoBehaviour
{
    [Header("Buttons")]
    public Button japanButton;
    public Button philippinesButton;   // correct
    public Button canadaButton;

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color correctColor = new Color32(120, 245, 180, 255);
    public Color wrongColor = new Color32(220, 60, 60, 255);

    [Header("Optional SFX")]
    public AudioSource sfx;
    public AudioClip correctSfx, wrongSfx;

    [Header("Popups")]
    public WrongPopup wrongPopup;
    public CoinPopup coinPopup;   // fallback only

    [Header("Flow (coins + then PLAY AGAIN)")]
    public NameTheFlagWinFlow winFlow;

    int wrongAttemptsThisRound = 0;

    void Start() => ResetRound();

    // Hook these in Button OnClick
    public void ChooseJapan() => Evaluate(japanButton, false);
    public void ChoosePhilippines() => Evaluate(philippinesButton, true);
    public void ChooseCanada() => Evaluate(canadaButton, false);

    public void ResetRound()
    {
        wrongAttemptsThisRound = 0;
        SetButtonVisual(japanButton, normalColor, true);
        SetButtonVisual(philippinesButton, normalColor, true);
        SetButtonVisual(canadaButton, normalColor, true);
    }

    void Evaluate(Button selected, bool isCorrect)
    {
        if (isCorrect)
        {
            int attempts = wrongAttemptsThisRound + 1; // 1=first try, 2=second, 3+=later
            int award = attempts <= 1 ? 10 : attempts == 2 ? 5 : 3;

            SetButtonVisual(selected, correctColor, false);
            LockOtherButtons(selected);

            if (sfx && correctSfx) sfx.PlayOneShot(correctSfx);

            // Prefer the centralized flow (will show coins, then PLAY AGAIN)
            if (winFlow)
            {
                winFlow.HandleWin(attempts);
            }
            else
            {
                // Fallback: show coins only (no PLAY AGAIN automation)
                if (coinPopup) coinPopup.Show(award);
            }
        }
        else
        {
            wrongAttemptsThisRound++;
            StartCoroutine(FlashWrongThenReset(selected));

            if (sfx && wrongSfx) sfx.PlayOneShot(wrongSfx);
            if (wrongPopup) wrongPopup.Show();
        }
    }

    IEnumerator FlashWrongThenReset(Button b)
    {
        SetButtonVisual(b, wrongColor, true);
        yield return new WaitForSeconds(0.25f);
        SetButtonVisual(b, normalColor, true);
    }

    void LockOtherButtons(Button keep)
    {
        if (japanButton && japanButton != keep) japanButton.interactable = false;
        if (philippinesButton && philippinesButton != keep) philippinesButton.interactable = false;
        if (canadaButton && canadaButton != keep) canadaButton.interactable = false;
    }

    void SetButtonVisual(Button b, Color color, bool interactable)
    {
        if (!b) return;

        if (b.targetGraphic) b.targetGraphic.color = color;

        var cb = b.colors;
        cb.normalColor = normalColor;
        cb.highlightedColor = normalColor;
        cb.pressedColor = normalColor;
        cb.selectedColor = normalColor;
        cb.disabledColor = color;     // how it looks when disabled
        b.colors = cb;

        b.interactable = interactable;
    }
}
