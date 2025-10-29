using UnityEngine;

public class SeeItWinFlow : MonoBehaviour
{
    [Header("Refs")]
    public SeeItTimerController timer;          // drag TimerController (with SeeItTimerController)
    public CharacterBouncer character;          // drag the moving character (must have public void Stop())
    public SeeItSimpleCoinPopup coinPopup;      // drag SeeItCoinPopup object (with SeeItSimpleCoinPopup)
    public CanvasGroup playAgainPanel;          // drag PLAYAGAIN CanvasGroup

    [Header("Timing")]
    [Tooltip("Delay after coin popup before PLAY AGAIN shows")]
    public float delayBeforePlayAgain = 0.7f;

    bool _done;

    /// <summary>Call this when 3/3 are found OR when the timer times out.</summary>
    public void HandleWin()
    {
        if (_done) return;
        _done = true;

        // 1) Freeze timer at its last shown value
        if (timer != null) timer.Pause();

        // 2) Stop character motion
        if (character != null) character.Stop();

        // 3) Show coin popup once
        if (coinPopup != null) coinPopup.ShowAward();

        // 4) After a small delay, show PLAY AGAIN (unscaled so it still runs if timescale changes)
        StartCoroutine(ShowPlayAgainAfter());
    }

    System.Collections.IEnumerator ShowPlayAgainAfter()
    {
        float t = 0f;
        while (t < delayBeforePlayAgain)
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        ShowPlayAgain();
    }

    void ShowPlayAgain()
    {
        if (!playAgainPanel) return;
        playAgainPanel.alpha = 1f;
        playAgainPanel.interactable = true;
        playAgainPanel.blocksRaycasts = true;
    }

    public void HidePlayAgain()
    {
        if (!playAgainPanel) return;
        playAgainPanel.alpha = 0f;
        playAgainPanel.interactable = false;
        playAgainPanel.blocksRaycasts = false;
    }
}
