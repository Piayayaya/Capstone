using UnityEngine;

public static class CoinWallet_DragDrop
{
    const string Key = "Coins_Total";
    public static int Total
    {
        get => PlayerPrefs.GetInt(Key, 0);
        private set { PlayerPrefs.SetInt(Key, value); PlayerPrefs.Save(); }
    }

    public static void Add(int amount)
    {
        if (amount <= 0) return;
        Total = Total + amount;
    }
}
