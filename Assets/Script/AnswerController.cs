using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class AnswerController : MonoBehaviour
{
    [Header("Buttons (3 options)")]
    public Button optionA;       
    public Button optionB;       
    public Button optionC;       
    public Button correctButton; 

    [Header("Colors")]
    public Color normalColor = Color.white;
    public Color correctColor = new Color32(120, 245, 180, 255);
    public Color wrongColor = new Color32(220, 60, 60, 255);

    [Header("Optional SFX")]
    public AudioSource sfx;
    public AudioClip correctSfx;
    public AudioClip wrongSfx;

    [Header("UI / Popups")]
    public SmallUIController ui; 

    int wrongAttempts = 0;

    void Start() => ResetRound();

    
    public void ChooseA() => Evaluate(optionA, optionA == correctButton);
    public void ChooseB() => Evaluate(optionB, optionB == correctButton);
    public void ChooseC() => Evaluate(optionC, optionC == correctButton);

    public void ResetRound()
    {
        wrongAttempts = 0;
        SetButtonVisual(optionA, normalColor, true);
        SetButtonVisual(optionB, normalColor, true);
        SetButtonVisual(optionC, normalColor, true);
    }

    void Evaluate(Button selected, bool isCorrect)
    {
        if (isCorrect)
        {
            int award = wrongAttempts == 0 ? 10 :
                        wrongAttempts == 1 ? 5 : 3;

            SetButtonVisual(selected, correctColor, false);
            LockOtherButtons(selected);

            if (sfx && correctSfx) sfx.PlayOneShot(correctSfx);
            if (ui) ui.ShowCoins(award);
        }
        else
        {
            wrongAttempts++;
            if (sfx && wrongSfx) sfx.PlayOneShot(wrongSfx);
            if (ui) ui.ShowWrong();
            StartCoroutine(FlashWrongThenBack(selected));
        }
    }

    IEnumerator FlashWrongThenBack(Button b)
    {
        SetButtonVisual(b, wrongColor, true);
        yield return new WaitForSeconds(0.25f);
        SetButtonVisual(b, normalColor, true);
    }

    void LockOtherButtons(Button keep)
    {
        if (optionA != keep) optionA.interactable = false;
        if (optionB != keep) optionB.interactable = false;
        if (optionC != keep) optionC.interactable = false;
    }

    void SetButtonVisual(Button b, Color faceColor, bool interactable)
    {
        if (!b) return;

        if (b.targetGraphic) b.targetGraphic.color = faceColor;

        var cb = b.colors;
        cb.normalColor = normalColor;
        cb.highlightedColor = normalColor;
        cb.pressedColor = normalColor;
        cb.selectedColor = normalColor;
        cb.disabledColor = faceColor; // how it looks when disabled
        b.colors = cb;

        b.interactable = interactable;
    }
}
