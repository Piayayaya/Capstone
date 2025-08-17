using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [SerializeField] private GameObject usernameInputPanel;
    [SerializeField] private InputField usernameInputField;
    [SerializeField] private Button submitButton;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Makes UIManager persistent across scenes
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        if (submitButton != null)
        {
            submitButton.onClick.AddListener(OnSubmitUsername);
        }
    }

    public void ShowUsernameInputPanel()
    {
        if (usernameInputPanel != null)
        {
            usernameInputPanel.SetActive(true);
        }
    }

    private void OnSubmitUsername()
    {
        string username = usernameInputField.text;

        if (!string.IsNullOrEmpty(username))
        {
            Player.Instance.SetUsername(username); // Sends the username to Player.cs for processing
            usernameInputPanel.SetActive(false); // Hide the panel after submitting
        }
    }
}
