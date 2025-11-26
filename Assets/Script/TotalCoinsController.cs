using TMPro;
using UnityEngine;

public class TotalCoinsController : MonoBehaviour
{
    [Header("Per-mode coin texts")]
    [SerializeField] private TMP_Text smartLadderCoinsText;
    [SerializeField] private TMP_Text nameTheFlagCoinsText;
    [SerializeField] private TMP_Text dragAndDropCoinsText;
    [SerializeField] private TMP_Text tuneYourTongueCoinsText;
    [SerializeField] private TMP_Text seeItOrLoseItCoinsText;
    [SerializeField] private TMP_Text dailyRewardsCoinsText; // NEW
    [SerializeField] private TMP_Text dailyQuestCoinsText;   // NEW

    [Header("Total coins text")]
    [SerializeField] private TMP_Text totalCoinsText;

    private void OnEnable()
    {
        if (CoinService.Instance == null) return;

        RefreshAll();

        CoinService.Instance.OnModeChanged += HandleModeChanged;
        CoinService.Instance.OnTotalChanged += HandleTotalChanged;
    }

    private void OnDisable()
    {
        if (CoinService.Instance == null) return;

        CoinService.Instance.OnModeChanged -= HandleModeChanged;
        CoinService.Instance.OnTotalChanged -= HandleTotalChanged;
    }

    private void RefreshAll()
    {
        if (CoinService.Instance == null) return;

        SetText(smartLadderCoinsText,
            CoinService.Instance.GetModeCoins(GameModeId.SmartLadder));

        SetText(nameTheFlagCoinsText,
            CoinService.Instance.GetModeCoins(GameModeId.NameTheFlag));

        SetText(dragAndDropCoinsText,
            CoinService.Instance.GetModeCoins(GameModeId.DragAndDrop));

        SetText(tuneYourTongueCoinsText,
            CoinService.Instance.GetModeCoins(GameModeId.TuneYourTongue));

        SetText(seeItOrLoseItCoinsText,
            CoinService.Instance.GetModeCoins(GameModeId.SeeItOrLoseIt));

        SetText(dailyRewardsCoinsText,
            CoinService.Instance.GetModeCoins(GameModeId.DailyRewards));

        SetText(dailyQuestCoinsText,
            CoinService.Instance.GetModeCoins(GameModeId.DailyQuests));

        SetText(totalCoinsText, CoinService.Instance.TotalCoins);
    }

    private void HandleModeChanged(GameModeId mode, int newValue)
    {
        // small project so easiest is to just refresh everything
        RefreshAll();
    }

    private void HandleTotalChanged(int newTotal)
    {
        SetText(totalCoinsText, newTotal);
    }

    private static void SetText(TMP_Text txt, int value)
    {
        if (txt != null)
            txt.text = value.ToString();
    }
}
