// RegisterUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public string nextScene = "Profile Scene";

    const string NameKey = "profile_name";

    public void OnClickCreate()
    {
        if (!nameInput)
        {
            Debug.LogError("RegisterUI.nameInput not assigned.");
            return;
        }

        var rawName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(rawName))
            rawName = "PLAYER";

        // 1) Store in your ProfileService (for the current session)
        if (ProfileService.Instance != null)
        {
            ProfileService.Instance.SetName(rawName);
        }

        // 2) Store in PlayerPrefs (for next sessions / other scenes)
        PlayerPrefs.SetString(NameKey, rawName);
        PlayerPrefs.Save();

        SceneManager.LoadScene(nextScene);
    }
}
