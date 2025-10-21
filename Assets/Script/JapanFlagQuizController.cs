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
    public Color wrongColor = new Color32(220, 60, 60, 255);   // red

    [Header("Popups")]
    public WrongPopup wrongPopup;      // your existing WrongPopup
    public JapanCoinPopup coinPopup;   // the script above

    int wrongTries = 0;

    void Start() => ResetRound();

    // Hook these to each button's OnClick
    public void ChooseFinland() => Evaluate(finlandButton, false);
    public void ChooseCanada() => Evaluate(canadaButton, false);
    public void ChooseJapan() => Evaluate(japanButton, true);

    public void ResetRound()
    {
        wrongTries = 0;
        SetButtonVisual(finlandButton, normalColor, true);
        SetButtonVisual(canadaButton, normalColor, true);
        SetButtonVisual(japanButton, normalColor, true);
    }

    void Evaluate(Button pressed, bool isCorrect)
    {
        if (isCorrect)
        {
            int award = wrongTries == 0 ? 10 : (wrongTries == 1 ? 5 : 3);

            // lock the other buttons and paint the correct one green
            SetButtonVisual(japanButton, correctColor, false);
            if (pressed != japanButton) SetButtonVisual(pressed, wrongColor, false); // safety
            LockOthers(japanButton);

            coinPopup?.Show(award);
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
        cb.disabledColor = color;   // how it looks when disabled
        b.colors = cb;

        b.interactable = interactable;
    }
}
