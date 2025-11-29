using TMPro;
using UnityEngine;

public class CoinHudBinder : MonoBehaviour
{
    [SerializeField] private TMP_Text coinsText;

    private void Awake()
    {
        if (!coinsText)
            coinsText = GetComponent<TMP_Text>();
    }

    private void OnEnable()
    {
        if (CoinService.Instance == null)
        {
            Debug.LogWarning("[CoinHudBinder] CoinService.Instance is null.");
            return;
        }

        Refresh();
        CoinService.Instance.OnTotalChanged += HandleTotalChanged;
    }

    private void OnDisable()
    {
        if (CoinService.Instance == null) return;
        CoinService.Instance.OnTotalChanged -= HandleTotalChanged;
    }

    private void HandleTotalChanged(int newTotal)
    {
        SetText(newTotal);
    }

    public void Refresh()
    {
        if (CoinService.Instance == null) return;
        SetText(CoinService.Instance.TotalCoins);
    }

    private void SetText(int value)
    {
        if (coinsText != null)
            coinsText.text = value.ToString("N0");
    }
}
