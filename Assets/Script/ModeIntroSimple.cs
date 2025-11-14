using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class ModeIntroSimple : MonoBehaviour
{
    public static ModeIntroSimple Instance { get; private set; }

    [Header("Wiring (assign these)")]
    public GameObject panelRoot;        // assign: ModeIntroPanel (this object)
    public CanvasGroup canvasGroup;     // CanvasGroup on ModeIntroPanel
    public TMP_Text messageText;        // Message (TMP)
    public Button proceedButton;        // ProceedButton
    public Button cancelButton;         // CancelButton

    [Header("Optional")]
    public string cancelSceneName = ""; // "Dashboard" to go there; empty = just close

    private Action _onProceed;

    void Awake()
    {
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;

        if (!panelRoot) panelRoot = gameObject;                 // fallback
        if (!canvasGroup) canvasGroup = GetComponent<CanvasGroup>(); // fallback

        if (proceedButton) proceedButton.onClick.AddListener(OnProceed);
        if (cancelButton) cancelButton.onClick.AddListener(OnCancel);

        HideInstant();
    }

    void HideInstant()
    {
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        if (panelRoot) panelRoot.SetActive(false);
    }

    public void Open(string message, Action onProceed)
    {
        Debug.Log("[ModeIntroSimple] Open");
        if (messageText) messageText.text = message ?? "";
        _onProceed = onProceed;

        if (panelRoot) panelRoot.SetActive(true);
        if (canvasGroup)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.blocksRaycasts = true;
            canvasGroup.interactable = true;
        }
    }

    public void Close()
    {
        Debug.Log("[ModeIntroSimple] Close");
        if (canvasGroup)
        {
            canvasGroup.alpha = 0f;
            canvasGroup.blocksRaycasts = false;
            canvasGroup.interactable = false;
        }
        if (panelRoot) panelRoot.SetActive(false);
    }

    void OnProceed()
    {
        Debug.Log("[ModeIntroSimple] Proceed clicked");
        var cb = _onProceed;
        Close();
        cb?.Invoke();
    }

    void OnCancel()
    {
        Debug.Log("[ModeIntroSimple] Cancel clicked");
        if (!string.IsNullOrEmpty(cancelSceneName))
            SceneManager.LoadScene(cancelSceneName);
        else
            Close();
    }
}
