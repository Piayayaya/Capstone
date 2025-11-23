using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class JapanFlagQuizController : MonoBehaviour
{
    [Header("Buttons")]
    public Button finlandButton;   // wrong
    public Button canadaButton;    // wrong
    public Button japanButton;     // correct

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color correctColor = new Color32(120, 245, 180, 255); // green
    public Color wrongColor = new Color32(220, 60, 60, 255);     // red

    [Header("Popups")]
    public WrongPopup wrongPopup;      // existing WrongPopup
    public CoinPopup coinPopup;        // ✅ SAME type as other scenes
    public NameTheFlagWinFlow winFlow; // ✅ coins + PLAY AGAIN flow

    private int wrongTries = 0;
    private bool roundDone = false;

    void Awake()
    {
        // Auto-find if not wired (safe for any NTF scene)
        if (wrongPopup == null)
            wrongPopup = FindObjectOfType<WrongPopup>(true);

        if (coinPopup == null)
            coinPopup = FindObjectOfType<CoinPopup>(true);

        if (winFlow == null)
            winFlow = FindObjectOfType<NameTheFlagWinFlow>(true);
    }

    void Start() => ResetRound();

    // Hook these to each button's OnClick
    public void ChooseFinland() => Evaluate(finlandButton, false);
    public void ChooseCanada() => Evaluate(canadaButton, false);
    public void ChooseJapan() => Evaluate(japanButton, true);

    public void ResetRound()
    {
        wrongTries = 0;
        roundDone = false;

        SetButtonVisual(finlandButton, normalColor, true);
        SetButtonVisual(canadaButton, normalColor, true);
        SetButtonVisual(japanButton, normalColor, true);
    }

    void Evaluate(Button pressed, bool isCorrect)
    {
        if (roundDone) return;

        if (isCorrect)
        {
            roundDone = true;

            int attempts = wrongTries + 1; // 1=first try, 2=second, 3+=later

            // paint correct one green + lock all buttons
            SetButtonVisual(japanButton, correctColor, false);
            LockOthers(japanButton);

            // ✅ Show coins + then PlayAgain panel through WinFlow
            if (winFlow != null)
            {
                winFlow.HandleWin(attempts);
            }
            else
            {
                // fallback if winFlow missing
                int award = attempts <= 1 ? 10 : (attempts == 2 ? 5 : 3);
                coinPopup?.Show(award);
            }
        }
        else
        {
            wrongTries++;
            StartCoroutine(FlashWrongThenReset(pressed));
            wrongPopup?.Show();
        }
    }

    IEnumerator FlashWrongThenReset(Button b)
    {
        SetButtonVisual(b, wrongColor, true);
        yield return new WaitForSeconds(0.25f);
        SetButtonVisual(b, normalColor, true);
    }

    void LockOthers(Button keep)
    {
        if (finlandButton != keep) finlandButton.interactable = false;
        if (canadaButton != keep) canadaButton.interactable = false;
        if (japanButton != keep) japanButton.interactable = false;
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
        cb.disabledColor = color;
        b.colors = cb;

        b.interactable = interactable;
    }
}
