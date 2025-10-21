using UnityEngine;

public class SmallUIController : MonoBehaviour
{
    [Header("Refs")]
    public WrongPopup wrongPopup;   // drag WrongPopUp (root with CanvasGroup + WrongPopup script)
    public CoinPopup coinPopup;     // drag CoinPopUp (root with CanvasGroup + CoinPopup script)

    public void ShowWrong()
    {
        if (wrongPopup) wrongPopup.Show();
    }

    public void ShowCoins(int amount)
    {
        if (coinPopup) coinPopup.Show(amount);
    }
}
