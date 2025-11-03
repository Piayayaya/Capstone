using TMPro;
using UnityEngine;

public class CoinHudBinder : MonoBehaviour
{
    public TMP_Text coinsText;

    public void Refresh()
    {
        if (coinsText) coinsText.text = ShopSave.Data.coinBalance.ToString("N0");
    }

    void OnEnable() => Refresh();
}
