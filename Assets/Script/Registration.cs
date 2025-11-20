using UnityEngine;
using TMPro;

public class Registration : MonoBehaviour
{
    public TMP_InputField usernameInput;
    public DatabaseService db;

    public async void OnCreatePressed()
    {
        string username = usernameInput.text;

        if (string.IsNullOrEmpty(username))
        {
            Debug.LogWarning("⚠ Username is empty");
            return;
        }

        string userId = System.Guid.NewGuid().ToString();
        PlayerPrefs.SetString("userId", userId);

        await db.CreateUser(userId, username);

        Debug.Log("👌 User Created: " + username);
    }
}
