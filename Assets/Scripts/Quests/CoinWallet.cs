using UnityEngine;
using System;

public class CoinWallet : MonoBehaviour
{
    public static CoinWallet I { get; private set; }
    const string KEY = "BM_Coins_v1";
    public int Coins { get; private set; }
    public System.Action OnCoinsChanged;


    void Awake()
    {
        if (I != null && I != this) { Destroy(gameObject); return; }
        I = this; DontDestroyOnLoad(gameObject);
        Coins = PlayerPrefs.GetInt(KEY, 0);
    }

    public void Add(int amount)
    {
        if (amount <= 0) return;
        Coins += amount;
        PlayerPrefs.SetInt(KEY, Coins);
        PlayerPrefs.Save();
        OnCoinsChanged?.Invoke();

    }
}
