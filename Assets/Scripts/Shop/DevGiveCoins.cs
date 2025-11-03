using UnityEngine;

public class DevGiveCoins : MonoBehaviour
{
    public int startingCoins = 5000;

    void Awake()
    {
        if (ShopSave.Data.coinBalance < startingCoins)
            ShopSave.AddCoins(startingCoins);
    }
}
