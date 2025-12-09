using System.Collections;
using System.Text;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

/// <summary>
/// Parent / OTP approval gate shown before any real-money purchase.
/// This does NOT charge real money – it just simulates an SMS/email OTP.
/// If otpApiUrl is set, it will call a backend (Apps Script / Firebase Function) that emails the OTP.
/// </summary>
public class ParentApprovalPanel : MonoBehaviour
{
    [Header("Hide While Parent Panel Is Open")]
    [SerializeField] private GameObject[] hideWhileOpen;

    [Header("UI Root")]
    public GameObject panelRoot;      // whole panel root

    [Header("Item Info")]
    public TMP_Text itemNameText;
    public TMP_Text priceText;

    [Header("Payment / Parent Info (INPUTS)")]
    public TMP_Dropdown paymentMethodDropdown;      // e.g. options: GCash, Maya, Credit Card
    public TMP_InputField accountNumberInput;       // e.g. GCash number
    public TMP_InputField parentEmailInput;         // parent Gmail / email

    [Header("Info / Status")]
    public TMP_Text methodDisplayText;              // shows "GCash • 09xx…" (optional)
    public TMP_Text infoText;                       // instructions + status
    public TMP_InputField otpInput;

    [Header("Buttons")]
    public Button sendCodeButton;
    public Button confirmButton;
    public Button cancelButton;

    [Header("Links")]
    public ShopAPI shopAPI;                         // drag your ShopAPI here in Inspector

    [Header("Email OTP API")]
    [Tooltip("HTTP endpoint that sends OTP email.")]
    public string otpApiUrl = "";   // set this in Inspector to your Apps Script URL

    [Header("Debug / Demo")]
    [Tooltip("If true, also show the OTP on-screen (good for testing/demo).")]
    public bool showOtpOnScreen = true;

    // --- internal state ---
    private LocalShopItem _pendingDbItem;
    private ItemDefinition _pendingSoItem;
    private ConfirmPanel _originPanel;
    private int _currentOtp;
    private bool _otpGenerated = false;

    [System.Serializable]
    private class OtpResponse
    {
        public bool ok;
        public string otp;
        public string error;
    }

    private void Awake()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        if (sendCodeButton != null)
            sendCodeButton.onClick.AddListener(OnSendCode);

        if (confirmButton != null)
            confirmButton.onClick.AddListener(OnConfirm);

        if (cancelButton != null)
            cancelButton.onClick.AddListener(OnCancel);
    }

    // ================= PUBLIC ENTRY POINTS =================

    public void StartDbPurchase(LocalShopItem item, ConfirmPanel origin, string defaultMethodLabel = "GCash / Maya")
    {
        _pendingDbItem = item;
        _pendingSoItem = null;
        _originPanel = origin;

        OpenPanel(
            item.ItemName ?? item.RefId,
            $"{item.PricePhp} PHP",
            defaultMethodLabel
        );
    }

    public void StartSoPurchase(ItemDefinition item, ConfirmPanel origin, string defaultMethodLabel = "GCash / Maya")
    {
        _pendingDbItem = null;
        _pendingSoItem = item;
        _originPanel = origin;

        string price = string.IsNullOrWhiteSpace(item.pesoDisplay)
            ? "PHP"
            : $"{item.pesoDisplay} PHP";

        OpenPanel(item.displayName, price, defaultMethodLabel);
    }

    // ======================================================

    private void OpenPanel(string itemName, string price, string defaultMethodLabel)
    {
        if (itemNameText) itemNameText.text = itemName;
        if (priceText) priceText.text = price;

        // payment method label
        if (paymentMethodDropdown != null && paymentMethodDropdown.options.Count > 0)
            paymentMethodDropdown.value = 0; // first option (e.g. GCash)

        if (methodDisplayText) methodDisplayText.text = defaultMethodLabel;

        _otpGenerated = false;
        _currentOtp = 0;

        if (otpInput) otpInput.text = string.Empty;
        if (accountNumberInput) accountNumberInput.text = string.Empty;
        if (parentEmailInput) parentEmailInput.text = string.Empty;

        if (infoText)
            infoText.text = "Ask your parent to enter their details and tap 'Send Code'.";

        if (panelRoot != null)
            panelRoot.SetActive(true);

        // hide store UI behind the panel
        SetHiddenObjects(true);

        transform.SetAsLastSibling();
    }

    private void ClosePanel()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);

        // show store UI again
        SetHiddenObjects(false);

        _pendingDbItem = null;
        _pendingSoItem = null;
        _originPanel = null;
        _otpGenerated = false;
        _currentOtp = 0;

        if (otpInput) otpInput.text = string.Empty;
    }

    private void SetHiddenObjects(bool hide)
    {
        if (hideWhileOpen == null) return;
        foreach (var go in hideWhileOpen)
        {
            if (go) go.SetActive(!hide);
        }
    }

    // ================== BUTTON HANDLERS ====================

    private void OnSendCode()
    {
        string email = parentEmailInput ? parentEmailInput.text.Trim() : "";
        string number = accountNumberInput ? accountNumberInput.text.Trim() : "";

        // basic email validation
        if (string.IsNullOrEmpty(email) || !IsValidEmail(email))
        {
            if (infoText) infoText.text = "Please enter a valid parent email.";
            return;
        }

        // ---- account number checks (digits only & exactly 11 digits) ----
        if (string.IsNullOrEmpty(number))
        {
            if (infoText) infoText.text = "Please enter the payment number (e.g. GCash).";
            return;
        }

        if (!IsDigitsOnly(number))
        {
            if (infoText) infoText.text = "Account number must contain digits only.";
            return;
        }

        if (number.Length != 11)
        {
            if (infoText) infoText.text = "Account number must be 11 digits.";
            return;
        }
        // -----------------------------------------------------------------

        // Update visible method label (GCash • 09xx1234xxx)
        if (methodDisplayText)
        {
            string methodName = paymentMethodDropdown != null
                ? paymentMethodDropdown.options[paymentMethodDropdown.value].text
                : "GCash / Maya";

            string maskedNumber = number.Length > 4
                ? new string('x', number.Length - 4) + number.Substring(number.Length - 4)
                : number;

            methodDisplayText.text = $"{methodName} • {maskedNumber}";
        }

        // If we have an API URL, ask the server to generate + email the OTP.
        if (!string.IsNullOrEmpty(otpApiUrl))
        {
            StartCoroutine(SendOtpRequest(email));
        }
        else
        {
            // Pure local/demo: generate OTP locally and just show it on screen
            GenerateLocalOtp(email, true);
        }
    }

    private void OnConfirm()
    {
        if (!_otpGenerated)
        {
            if (infoText)
                infoText.text = "Please tap 'Send Code' first.";
            return;
        }

        if (otpInput == null || string.IsNullOrWhiteSpace(otpInput.text))
        {
            if (infoText)
                infoText.text = "Please enter the code.";
            return;
        }

        if (!int.TryParse(otpInput.text.Trim(), out int typed) || typed != _currentOtp)
        {
            if (infoText)
                infoText.text = "Wrong code. Please try again or tap 'Send Code' again.";
            return;
        }

        // === CODE CORRECT – "approve payment" ===

        // 1) find which payment method the parent chose
        string selectedMethod = "Unknown";
        if (paymentMethodDropdown != null && paymentMethodDropdown.options.Count > 0)
        {
            selectedMethod = paymentMethodDropdown.options[paymentMethodDropdown.value].text;
        }

        // 2) (optional) open GCash / Maya app or website
        LaunchExternalPaymentApp(selectedMethod);

        // 3) grant the item inside BrainyMe (your existing logic)
        if (shopAPI == null)
        {
            Debug.LogWarning("[ParentApprovalPanel] shopAPI is null – cannot grant purchase.");
        }
        else
        {
            if (_pendingDbItem != null)
            {
                shopAPI.BuyPesoProductMock(_pendingDbItem.RefId);
            }
            else if (_pendingSoItem != null)
            {
                shopAPI.BuyPesoProductMock(_pendingSoItem.id);
            }
        }

        // ---- log a receipt notification for the parent / player ----
        LogPurchaseNotification(selectedMethod);

        // Tell ConfirmPanel to show success + refresh UI
        if (_originPanel != null)
        {
            _originPanel.OnExternalPaymentSuccess();
        }

        ClosePanel();
    }

    private void OnCancel()
    {
        ClosePanel();
    }

    // ================== EMAIL / OTP HELPERS ====================

    private bool IsValidEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email)) return false;
        const string pattern = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
        return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase);
    }

    // digits-only check for account number
    private bool IsDigitsOnly(string value)
    {
        if (string.IsNullOrEmpty(value)) return false;
        for (int i = 0; i < value.Length; i++)
        {
            if (!char.IsDigit(value[i])) return false;
        }
        return true;
    }

    /// <summary>
    /// Local/demo OTP generation. Used if no server or server fails.
    /// </summary>
    private void GenerateLocalOtp(string email, bool logReason)
    {
        _currentOtp = Random.Range(100000, 999999);
        _otpGenerated = true;

        if (logReason)
            Debug.Log("[ParentApprovalPanel] Using local/demo OTP. OTP = " + _currentOtp);

        UpdateInfoAfterOtpGenerated(email, showOtpOnScreen);
    }

    private void UpdateInfoAfterOtpGenerated(string email, bool showCode)
    {
        string maskedEmail = MaskEmail(email);
        if (infoText)
        {
            if (showCode)
            {
                infoText.text =
                    $"We sent a 6-digit code to {maskedEmail}.\n" +
                    $"(Demo code for parent: {_currentOtp})";
            }
            else
            {
                infoText.text = $"We sent a 6-digit code to {maskedEmail}.";
            }
        }
    }

    /// <summary>
    /// Sends { email } to your web app and expects JSON { ok, otp }.
    /// </summary>
    private IEnumerator SendOtpRequest(string email)
    {
        if (infoText) infoText.text = "Sending code…";

        // Build JSON: { "email": "parent@example.com" }
        string payload = "{\"email\":\"" + email + "\"}";
        byte[] bodyRaw = Encoding.UTF8.GetBytes(payload);

        using (UnityWebRequest req = new UnityWebRequest(otpApiUrl, "POST"))
        {
            req.uploadHandler = new UploadHandlerRaw(bodyRaw);
            req.downloadHandler = new DownloadHandlerBuffer();
            req.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("[ParentApprovalPanel] Sending OTP request to " + otpApiUrl);

            yield return req.SendWebRequest();

#if UNITY_2020_1_OR_NEWER
            bool error = req.result != UnityWebRequest.Result.Success;
#else
            bool error = req.isNetworkError || req.isHttpError;
#endif

            if (error)
            {
                Debug.LogError("[ParentApprovalPanel] OTP request error: " +
                               req.responseCode + " / " + req.error + " / " + req.downloadHandler.text);

                if (infoText)
                    infoText.text = "Failed to send code. Using offline demo code.";

                // Fall back to local/demo OTP: new code and show it on screen
                GenerateLocalOtp(email, true);
                yield break;
            }

            string response = req.downloadHandler.text;
            Debug.Log("[ParentApprovalPanel] OTP response: " + response);

            OtpResponse resObj = null;
            try
            {
                resObj = JsonUtility.FromJson<OtpResponse>(response);
            }
            catch
            {
                resObj = null;
            }

            if (resObj != null && resObj.ok && !string.IsNullOrEmpty(resObj.otp))
            {
                if (int.TryParse(resObj.otp, out var serverOtp))
                {
                    _currentOtp = serverOtp;
                    _otpGenerated = true;
                    Debug.Log("[ParentApprovalPanel] Server OTP = " + _currentOtp);
                }
                else
                {
                    GenerateLocalOtp(email, true);
                    yield break;
                }

                UpdateInfoAfterOtpGenerated(email, showOtpOnScreen);
            }
            else
            {
                Debug.LogWarning("[ParentApprovalPanel] Server did not return valid OTP. Falling back to local.");
                GenerateLocalOtp(email, true);
            }
        }
    }

    // ============= PAYMENT APP LAUNCHER =================

    private void LaunchExternalPaymentApp(string methodName)
    {
#if UNITY_ANDROID && !UNITY_EDITOR
        if (string.IsNullOrEmpty(methodName))
        {
            Debug.Log("[ParentApprovalPanel] No payment method selected.");
            return;
        }

        string lower = methodName.ToLower();

        if (lower.Contains("gcash"))
        {
            Application.OpenURL("https://www.gcash.com/");
            Debug.Log("[ParentApprovalPanel] Opening GCash website/app.");
        }
        else if (lower.Contains("maya"))
        {
            Application.OpenURL("https://www.maya.ph/");
            Debug.Log("[ParentApprovalPanel] Opening Maya website/app.");
        }
        else
        {
            Debug.Log("[ParentApprovalPanel] Payment method selected: " + methodName +
                      " (no external launch configured).");
        }
#else
        Debug.Log("[ParentApprovalPanel] Would open payment app: " + methodName);
#endif
    }

    // ============= RECEIPT NOTIFICATION =============

    private void LogPurchaseNotification(string paymentMethod)
    {
        // use your existing NotificationService.Add(...)
        if (NotificationService.Instance == null) return;

        string itemName = itemNameText ? itemNameText.text : "Unknown item";
        string price = priceText ? priceText.text : "";
        string accNumber = accountNumberInput ? accountNumberInput.text : "";

        if (string.IsNullOrEmpty(paymentMethod))
            paymentMethod = "Unknown method";

        string msg = $"Parent approved purchase: {itemName} ({price}) via {paymentMethod} ({accNumber}).";

        NotificationService.Instance.Add(msg);
    }

    // =====================================================

    private static string MaskEmail(string email)
    {
        int at = email.IndexOf('@');
        if (at <= 1) return email;

        string first = email.Substring(0, 1);
        string domain = email.Substring(at);
        return first + "***" + domain;
    }
}
