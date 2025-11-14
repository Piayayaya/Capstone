using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConfirmSellPopup : MonoBehaviour
{
    public static ConfirmSellPopup Instance;

    [Header("Wiring")]
    public CanvasGroup cg;
    public TMP_Text messageText;
    public Button confirmButton;
    public TMP_Text confirmButtonLabel;
    public Button cancelButton;

    Action _onConfirm;

    void Awake()
    {
        Instance = this;
        HideImmediate();

        confirmButton.onClick.AddListener(() =>
        {
            Hide();
            _onConfirm?.Invoke();
        });

        cancelButton.onClick.AddListener(Hide);
    }

    void HideImmediate()
    {
        if (!cg) return;
        cg.alpha = 0f;
        cg.blocksRaycasts = false;
        cg.interactable = false;
        gameObject.SetActive(false);
    }

    void ShowInternal()
    {
        gameObject.SetActive(true);
        if (!cg) return;
        cg.alpha = 1f;
        cg.blocksRaycasts = true;
        cg.interactable = true;
    }

    void Hide()
    {
        HideImmediate(); // simple hide (no animation to keep code short)
    }

    public static void Show(string message, string confirmLabel, Action onConfirm)
    {
        if (!Instance) { Debug.LogError("[ConfirmSellPopup] Missing Instance in scene."); return; }
        Instance.messageText.text = message;
        Instance.confirmButtonLabel.text = confirmLabel;
        Instance._onConfirm = onConfirm;
        Instance.ShowInternal();
    }
}
