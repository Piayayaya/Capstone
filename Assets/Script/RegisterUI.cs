// RegisterUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public string nextScene = "Profile Scene";

    const string NameKeyBase = "profile_name";

    public async void OnClickCreate()
    {
        string rawName = nameInput.text.Trim();
        if (string.IsNullOrEmpty(rawName))
            rawName = "PLAYER";

        // ✅ Use stable guest id for this device
        string userId = UserIdProvider.GetOrCreateGuestId();

        // Save in ProfileService
        if (ProfileService.Instance != null)
            ProfileService.Instance.SetName(rawName);

        // ✅ Save locally per user
        string perUserNameKey = $"{NameKeyBase}_{userId}";
        PlayerPrefs.SetString(perUserNameKey, rawName);
        PlayerPrefs.SetString(NameKeyBase, rawName); // backward compat
        PlayerPrefs.Save();

        // Save to Firebase
        if (DatabaseService.Instance != null)
            await DatabaseService.Instance.CreateUser(userId, rawName);

        Debug.Log("🔥 User saved + UI registered");

        SceneManager.LoadScene(nextScene);
    }
}
