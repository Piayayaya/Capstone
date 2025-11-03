using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// Attach this to your Daily Rewards modal/panel. Keep the panel disabled in the scene.
/// It will auto-show on Dashboard load if the player hasn't claimed today's reward yet.
public class DailyLoginPopup : MonoBehaviour
{
    [Header("Wiring")]
    [Tooltip("Root object of this popup (the panel you want to show/hide).")]
    public GameObject popupRoot;

    [Tooltip("Optional: title or info text to show current streak / status.")]
    public TMP_Text infoText;

    [Tooltip("Buttons for Day 1..7 (enable only the claimable one).")]
    public Button[] dayButtons;          // size 7 (index 0 = Day1, ..., 6 = Day7)
    public TMP_Text[] dayButtonLabels;   // optional: label text for each button (e.g., 'CLAIM', 'CLAIMED')

    [Header("Rewards")]
    [Tooltip("Coins (or whatever) per day 1..7. Adjust as you like.")]
    public int[] coinRewards = new int[7] { 50, 75, 100, 125, 150, 200, 300 };

    [Header("Behavior")]
    [Tooltip("If true, modal appears on Dashboard even if already claimed today (useful if you always want to show it first).")]
    public bool alwaysShowOnOpen = false;

    [Header("FX / Feedback")]
    public RewardToast rewardToast;   // drag your RewardToastPanel here

    // PlayerPrefs keys
    const string KEY_LAST_CLAIM_UTC = "dl_last_claim_utc";
    const string KEY_STREAK = "dl_streak";

    // Session guard so it only auto-opens once per app launch (optional)
    private static bool _shownThisSession = false;

    void Awake()
    {
        if (popupRoot != null) popupRoot.SetActive(false);
        // Wire all day buttons to a single handler
        for (int i = 0; i < dayButtons.Length; i++)
        {
            int dayIndex = i; // capture
            if (dayButtons[i] != null)
                dayButtons[i].onClick.AddListener(() => OnClickClaim(dayIndex));
        }
    }

    void Start()
    {
        // Decide whether to show immediately
        var data = LoadProgressAndUpdateStreak();

        bool claimedToday = data.claimedToday;
        bool shouldShow = alwaysShowOnOpen || !claimedToday;

        if (!_shownThisSession && shouldShow)
        {
            _shownThisSession = true;
            Open(data);
        }
        else
        {
            Close(); // just to be sure
        }
    }

    /// Opens and refreshes UI
    public void Open(DailyState state = default)
    {
        if (popupRoot != null) popupRoot.SetActive(true);
        RefreshUI(state.Equals(default(DailyState)) ? LoadProgressAndUpdateStreak() : state);
    }

    public void Close()
    {
        if (popupRoot != null) popupRoot.SetActive(false);
    }

    // --- Core logic ---

    public struct DailyState
    {
        public int streak;           // 1..7 with weekly loop (or keep growing if you prefer)
        public bool claimedToday;
        public int claimableIndex;   // 0..6 which button can be claimed now; -1 if none
        public DateTime lastClaimUtcDate;
    }

    DailyState LoadProgressAndUpdateStreak()
    {
        int streak = PlayerPrefs.GetInt(KEY_STREAK, 0);
        string lastClaimStr = PlayerPrefs.GetString(KEY_LAST_CLAIM_UTC, "");
        DateTime lastClaimDate = DateTime.MinValue;

        if (!string.IsNullOrEmpty(lastClaimStr))
        {
            // stored as yyyy-MM-dd
            DateTime.TryParse(lastClaimStr, out lastClaimDate);
        }

        DateTime today = DateTime.UtcNow.Date;
        bool claimedToday = (lastClaimDate.Date == today);

        // Update streak if a new day has started
        if (!claimedToday && lastClaimDate != DateTime.MinValue)
        {
            int diffDays = (int)(today - lastClaimDate.Date).TotalDays;
            if (diffDays == 1)
            {
                // consecutive login -> increment streak
                streak++;
            }
            else if (diffDays > 1)
            {
                // missed at least one day -> reset streak
                streak = 1;
            }
        }
        else if (lastClaimDate == DateTime.MinValue)
        {
            // first time ever opening
            streak = 1;
        }

        // Cap/loop streak for 7-day calendar visual
        if (streak <= 0) streak = 1;
        if (streak > 7) streak = ((streak - 1) % 7) + 1;

        // If already claimed today, there’s nothing claimable now.
        int claimableIndex = claimedToday ? -1 : (streak - 1);

        // Persist normalized streak (don’t write today as claimed here—only on claim)
        PlayerPrefs.SetInt(KEY_STREAK, streak);
        PlayerPrefs.Save();

        return new DailyState
        {
            streak = streak,
            claimedToday = claimedToday,
            claimableIndex = claimableIndex,
            lastClaimUtcDate = lastClaimDate
        };
    }

    void RefreshUI(DailyState state)
    {
        // Info text
        if (infoText != null)
        {
            infoText.text = state.claimedToday
                ? $"You’ve already claimed today! (Streak: Day {state.streak})"
                : $"Day {state.streak} reward is ready!";
        }

        // Enable only the claimable button; mark others as disabled/claimed
        for (int i = 0; i < dayButtons.Length; i++)
        {
            bool isClaimable = (i == state.claimableIndex);
            bool isClaimedToday = state.claimedToday && (i == state.streak - 1);

            if (dayButtons[i] != null)
                dayButtons[i].interactable = isClaimable;

            if (dayButtonLabels != null && i < dayButtonLabels.Length && dayButtonLabels[i] != null)
            {
                if (isClaimable) dayButtonLabels[i].text = "CLAIM";
                else if (isClaimedToday) dayButtonLabels[i].text = "CLAIMED";
                else dayButtonLabels[i].text = "LOCKED";
            }
        }
    }

    void OnClickClaim(int dayIndex)
    {
        var state = LoadProgressAndUpdateStreak();
        if (dayIndex != state.claimableIndex)
        {
            // Not allowed
            return;
        }

        int reward = 0;
        if (coinRewards != null && coinRewards.Length == 7)
            reward = coinRewards[dayIndex];

        // TODO: Hook into your wallet/inventory here
        // Example:
        // Wallet.Instance.AddCoins(reward);
        Debug.Log($"Daily reward claimed: Day {dayIndex + 1} -> +{reward} coins");

        // Disable all buttons to prevent double-claim while toast shows (optional)
        foreach (var b in dayButtons) if (b) b.interactable = false;

        // Show toast; when it finishes, close the daily popup
        if (rewardToast)
        {
            rewardToast.Show(reward, () =>
            {
                Close(); // hides popupRoot so player can navigate
            });
        }
        else
        {
            // Fallback: if no toast, close immediately
            Close();
        }


        // Mark today as claimed
        string todayStr = DateTime.UtcNow.Date.ToString("yyyy-MM-dd");
        PlayerPrefs.SetString(KEY_LAST_CLAIM_UTC, todayStr);
        PlayerPrefs.Save();

        // Refresh UI to show claimed state
        state.claimedToday = true;
        state.claimableIndex = -1;
        RefreshUI(state);

        // Optional: auto-close after claiming
        // Close();
    }

    // Utility you can call from a menu/debug button to reset progress
    public void Debug_ResetDaily()
    {
        PlayerPrefs.DeleteKey(KEY_LAST_CLAIM_UTC);
        PlayerPrefs.DeleteKey(KEY_STREAK);
        PlayerPrefs.Save();
        _shownThisSession = false;
        Debug.Log("Daily login data reset.");
    }


}

