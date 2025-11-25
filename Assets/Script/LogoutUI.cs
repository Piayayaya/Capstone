using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class LogoutUI : MonoBehaviour
{
    [Header("Target Scene After Sign Out")]
    public string createAccountScene = "CreateAccount";

    [Header("Confirm Panel")]
    [Tooltip("CanvasGroup on your 'Are you sure?' panel")]
    [SerializeField] private CanvasGroup confirmPanel;

    [Tooltip("Optional text inside the confirm panel")]
    [SerializeField] private TMP_Text confirmMessageText;

    [TextArea]
    [SerializeField] private string confirmMessage = "Are you sure you want to leave the game?";

    private void Start()
    {
        HideConfirm();
    }

    // ===== PUBLIC METHODS TO HOOK IN INSPECTOR =====

    // 1) Hook THIS to the Sign Out button (instead of Logout)
    public void OnClickSignOutButton()
    {
        ShowConfirm();
    }

    // 2) Hook this to the YES button in the confirm panel
    public void OnClickConfirmYes()
    {
        HideConfirm();
        Logout();   // actual sign-out
    }

    // 3) Hook this to the NO button in the confirm panel
    public void OnClickConfirmNo()
    {
        HideConfirm();
    }

    // ===== EXISTING LOGOUT LOGIC (unchanged) =====

    // Actual sign-out action
    public void Logout()
    {
        Debug.Log("[LogoutUI] Sign out clicked -> going to CreateAccount");

        // Prevent auto-routing back immediately
        AutoLoginRouter.SkipAutoRouteOnce = true;

        // DO NOT clear local or firebase.
        SceneManager.LoadScene(createAccountScene);
    }

    // ===== INTERNAL HELPERS =====

    private void ShowConfirm()
    {
        if (confirmPanel == null)
        {
            // If no panel is assigned, just logout as before.
            Logout();
            return;
        }

        confirmPanel.gameObject.SetActive(true);
        confirmPanel.alpha = 1f;
        confirmPanel.interactable = true;
        confirmPanel.blocksRaycasts = true;

        if (confirmMessageText != null)
            confirmMessageText.text = confirmMessage;
    }

    private void HideConfirm()
    {
        if (confirmPanel == null) return;

        confirmPanel.alpha = 0f;
        confirmPanel.interactable = false;
        confirmPanel.blocksRaycasts = false;
        confirmPanel.gameObject.SetActive(true); // keep active, we control via CanvasGroup
    }
}
