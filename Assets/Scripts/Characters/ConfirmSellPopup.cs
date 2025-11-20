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

        Debug.Log("[ConfirmSellPopup] Awake on: " + name, this);

        HideImmediate();

        confirmButton?.onClick.RemoveAllListeners();
        cancelButton?.onClick.RemoveAllListeners();

        confirmButton?.onClick.AddListener(() =>
        {
            Debug.Log("[ConfirmSellPopup] Confirm clicked", this);
            Hide();
            _onConfirm?.Invoke();
            _onConfirm = null;
        });

        cancelButton?.onClick.AddListener(() =>
        {
            Debug.Log("[ConfirmSellPopup] Cancel clicked", this);
            Hide();
        });
    }

    void HideImmediate()
    {
        // hide visually + disable interaction
        if (cg)
        {
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;
        }
        gameObject.SetActive(false);
    }

    void ShowInternal()
    {
        gameObject.SetActive(true);

        // FORCE TOP OF CANVAS
        transform.SetAsLastSibling();

        if (cg)
        {
            cg.alpha = 1f;
            cg.blocksRaycasts = true;
            cg.interactable = true;
        }

        Debug.Log("[ConfirmSellPopup] ShowInternal ACTIVE on: " + name, this);
    }

    void Hide() => HideImmediate();

    public static void Show(string message, string confirmLabel, Action onConfirm)
    {
        // fallback if Instance got lost
        if (!Instance)
            Instance = FindObjectOfType<ConfirmSellPopup>(true);

        if (!Instance)
        {
            Debug.LogError("[ConfirmSellPopup] No popup found in scene.");
            return;
        }

        Debug.Log("[ConfirmSellPopup] Show called on instance: " + Instance.name, Instance);

        if (!Instance.messageText)
        {
            Debug.LogError("[ConfirmSellPopup] messageText not wired!", Instance);
            return;
        }

        if (!Instance.confirmButtonLabel)
        {
            Debug.LogError("[ConfirmSellPopup] confirmButtonLabel not wired!", Instance);
            return;
        }

        Instance.messageText.text = message;
        Instance.confirmButtonLabel.text = confirmLabel;
        Instance._onConfirm = onConfirm;

        Instance.ShowInternal();
    }
}
