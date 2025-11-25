using System.Collections;
using UnityEngine;

public class NameTheFlagWinFlow : MonoBehaviour
{
    [Header("Refs (optional auto-find)")]
    public CoinPopup coinPopup;                // your existing CoinPopup
    public CanvasGroupFader playAgainPanel;    // the component on PLAYAGAIN

    [Header("Timing")]
    [Tooltip("Extra delay after the coin popup before showing PLAY AGAIN.")]
    public float delayBeforePlayAgain = 0.8f;

    [Header("Awards")]
    public int firstTryCoins = 10;
    public int secondTryCoins = 5;
    public int laterTryCoins = 3;

    private bool _showing;

    // NEW: make sure we only ever award once per round
    private bool _coinsAwarded = false;   // NEW

    void Awake()
    {
        // Auto-find if not wired
        if (coinPopup == null)
            coinPopup = FindObjectOfType<CoinPopup>(true);

        if (playAgainPanel == null)
            playAgainPanel = FindObjectOfType<CanvasGroupFader>(true);

        // Ensure panel starts hidden (safe for any scene)
        if (playAgainPanel != null)
            playAgainPanel.HideInstant();
        // If you don't have HideInstant(), comment this line.
    }

    // attempts: 1 = first try, 2 = second, 3+ = later
    public void HandleWin(int attempts = 1)
    {
        // NEW: if something tries to call this again, ignore it
        if (_coinsAwarded)
        {
            Debug.LogWarning("[NameTheFlagWinFlow] HandleWin called again – ignoring to avoid double coins.");
            return;
        }
        _coinsAwarded = true; // NEW

        if (_showing) return;
        _showing = true;

        int award = attempts <= 1 ? firstTryCoins
                  : attempts == 2 ? secondTryCoins
                  : laterTryCoins;

        // show old popup
        if (coinPopup) coinPopup.Show(award);

        // ✅ record coins for Name The Flag (works for ALL NTF scenes)
        if (CoinService.Instance != null)
        {
            Debug.Log($"[NameTheFlagWinFlow] Awarding {award} coins, attempts={attempts}"); // helpful log
            CoinService.Instance.AddCoins(award, GameModeId.NameTheFlag);
        }
        else
        {
            Debug.LogWarning("[NameTheFlagWinFlow] CoinService.Instance is null – coins not recorded.");
        }

        StartCoroutine(ShowPlayAgainAfterCoins());
    }

    IEnumerator ShowPlayAgainAfterCoins()
    {
        float wait =
            (coinPopup ? coinPopup.stay + coinPopup.fade * 2f : 0f)
            + delayBeforePlayAgain;

        yield return new WaitForSecondsRealtime(wait);

        if (playAgainPanel) playAgainPanel.Show();

        _showing = false;
        // NOTE: _coinsAwarded stays true for this round, which is what we want
    }
}
