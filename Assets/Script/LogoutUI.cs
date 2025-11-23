using UnityEngine;
using UnityEngine.SceneManagement;

public class LogoutUI : MonoBehaviour
{
    [Header("Target Scene After Sign Out")]
    public string createAccountScene = "CreateAccount";

    // Hook this to Sign Out button OnClick()
    public void Logout()
    {
        Debug.Log("[LogoutUI] Sign out clicked -> going to CreateAccount");

        // Prevent auto-routing back immediately
        AutoLoginRouter.SkipAutoRouteOnce = true;

        // DO NOT clear local or firebase.
        SceneManager.LoadScene(createAccountScene);
    }
}
