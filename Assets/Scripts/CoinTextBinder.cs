using TMPro;
using UnityEngine;

public class CoinTextBinder : MonoBehaviour
{
    public TMP_Text coinText;
    public CoinWallet wallet;

    void Start()
    {
        UpdateUI();
        wallet.OnCoinsChanged += UpdateUI;
    }

    void OnDestroy()
    {
        wallet.OnCoinsChanged -= UpdateUI;
    }

    void UpdateUI()
    {
        coinText.text = wallet.Coins.ToString();
    }
}
