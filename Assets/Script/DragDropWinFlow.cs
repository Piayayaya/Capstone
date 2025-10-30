// DragDropWinFlow.cs  (CanvasGroup-only version)
using System.Collections;
using UnityEngine;

public class DragDropWinFlow : MonoBehaviour
{
    [Header("Refs")]
    public CoinPopup_DragDrop coinPopup;   // your existing popup
    public CanvasGroup restartPanel;       // <- drag the CanvasGroup on RESTARTPANEL here

    [Header("Award logic")]
    public int coinsFirstTry = 10;
    public int coinsSecondTry = 5;
    public int coinsOtherTries = 3;

    [Header("Timing")]
    [Tooltip("Extra delay after the popup fully finishes before showing Restart.")]
    public float delayAfterPopup = 0.2f;

    int wrongTries = 0;
    bool done;

    void Awake()
    {
        // Ensure the panel starts hidden
        if (restartPanel)
        {
            restartPanel.alpha = 0f;
            restartPanel.interactable = false;
            restartPanel.blocksRaycasts = false;
        }
    }

    /// Call this when the player drops the right item in the right slot
    public void HandleWin()
    {
        if (done) return;
        done = true;

        int award = wrongTries == 0 ? coinsFirstTry :
                    wrongTries == 1 ? coinsSecondTry : coinsOtherTries;

        if (coinPopup) coinPopup.Show(award);

        float popupTotal = (coinPopup ? coinPopup.appearDuration + coinPopup.holdTime + coinPopup.fadeOutDuration : 0f);
        StartCoroutine(ShowRestartAfter(popupTotal + delayAfterPopup));
    }

    /// Call this on each wrong drop
    public void ReportWrongTry()
    {
        if (!done) wrongTries++;
    }

    IEnumerator ShowRestartAfter(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        ShowRestartPanel();
    }

    void ShowRestartPanel()
    {
        if (!restartPanel) return;
        restartPanel.alpha = 1f;
        restartPanel.interactable = true;
        restartPanel.blocksRaycasts = true;
    }

    // Optional helper if you want to hide it again
    public void HideRestartPanel()
    {
        if (!restartPanel) return;
        restartPanel.alpha = 0f;
        restartPanel.interactable = false;
        restartPanel.blocksRaycasts = false;
        done = false; // reset for next round
    }
}
