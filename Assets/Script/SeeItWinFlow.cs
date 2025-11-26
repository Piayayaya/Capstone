using System.Collections;
using UnityEngine;

public class SeeItWinFlow : MonoBehaviour
{
    [Header("Refs (optional auto-find)")]
    public SeeItTimerController timer;
    public SeeItCharacterPlayController characterPlay;
    public SeeItSimpleCoinPopup coinPopup;

    [Tooltip("If your PLAYAGAIN has CanvasGroupFader, drag it here.")]
    public CanvasGroupFader playAgainFader;

    [Tooltip("If no fader, drag PLAYAGAIN CanvasGroup here.")]
    public CanvasGroup playAgainGroup;

    [Header("Timing")]
    public float extraDelayBeforePlayAgain = 0.2f;

    [Header("Awards")]
    public int firstTryCoins = 10;
    public int secondTryCoins = 5;
    public int laterTryCoins = 3;

    private bool done;

    void Awake()
    {
        if (timer == null)
            timer = FindObjectOfType<SeeItTimerController>(true);

        if (characterPlay == null)
            characterPlay = FindObjectOfType<SeeItCharacterPlayController>(true);

        if (coinPopup == null)
            coinPopup = FindObjectOfType<SeeItSimpleCoinPopup>(true);

        if (playAgainFader == null)
            playAgainFader = FindObjectOfType<CanvasGroupFader>(true);

        if (playAgainGroup == null && playAgainFader == null)
            playAgainGroup = FindObjectOfType<CanvasGroup>(true);

        HidePlayAgainInstant();
    }

    // attempts = 1 first try, 2 second, 3+ later
    public void HandleWin(int attempts = 1)
    {
        if (done) return;
        done = true;

        // 1) stop timer
        if (timer != null)
            timer.StopTimer();

        // 2) stop character
        if (characterPlay != null)
            characterPlay.StopPlay();

        // 3) compute award
        int award = attempts <= 1 ? firstTryCoins
                  : attempts == 2 ? secondTryCoins
                  : laterTryCoins;

        // 🔹 NEW: add coins to the global CoinService (SeeItOrLoseIt mode)
        if (CoinService.Instance != null)
        {
            Debug.Log($"[SeeItWinFlow] Awarding {award} coins for SeeItOrLoseIt.");
            CoinService.Instance.AddCoins(award, GameModeId.SeeItOrLoseIt);
        }
        else
        {
            Debug.LogWarning("[SeeItWinFlow] CoinService.Instance is null – no coins added.");
        }

        // 4) show coin popup then playAgain
        if (coinPopup != null)
        {
            coinPopup.ShowAward(award);
            StartCoroutine(ShowPlayAgainAfterCoins());
        }
        else
        {
            StartCoroutine(ShowPlayAgainAfterDelay(1.0f));
        }
    }

    IEnumerator ShowPlayAgainAfterCoins()
    {
        float wait = (coinPopup != null ? coinPopup.TotalDuration : 1.2f)
                   + extraDelayBeforePlayAgain;

        yield return new WaitForSecondsRealtime(wait);
        ShowPlayAgain();
    }

    IEnumerator ShowPlayAgainAfterDelay(float wait)
    {
        yield return new WaitForSecondsRealtime(wait);
        ShowPlayAgain();
    }

    void HidePlayAgainInstant()
    {
        if (playAgainFader != null)
        {
            playAgainFader.HideInstant();
            return;
        }

        if (playAgainGroup != null)
        {
            playAgainGroup.alpha = 0f;
            playAgainGroup.interactable = false;
            playAgainGroup.blocksRaycasts = false;
        }
    }

    void ShowPlayAgain()
    {
        if (playAgainFader != null)
        {
            playAgainFader.Show();
            return;
        }

        if (playAgainGroup != null)
        {
            playAgainGroup.alpha = 1f;
            playAgainGroup.interactable = true;
            playAgainGroup.blocksRaycasts = true;
        }
    }
}
