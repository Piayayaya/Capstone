using System.Threading.Tasks;
using System.Collections.Generic;   // ✅ ADD THIS
using UnityEngine;
using Firebase.Database;

public class DatabaseService : MonoBehaviour
{
    public static DatabaseService Instance;
    private DatabaseReference db;

    private async void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            await StartDatabaseService();
        }
        else Destroy(gameObject);
    }

    private async Task StartDatabaseService()
    {
        await Task.Delay(500);
        db = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("DatabaseService initialized");
    }

    // ======================================================
    // CREATE USER (manual/guest)
    // ======================================================
    public async Task CreateUser(string userId, string username)
    {
        if (db == null) return;

        var data = new UserModel
        {
            name = username,
            createdAt = System.DateTime.Now.ToString("o")
        };

        string json = JsonUtility.ToJson(data);
        await db.Child("users").Child(userId).SetRawJsonValueAsync(json);

        Debug.Log("✔ User saved to Firebase");
    }

    // ======================================================
    // CREATE OR UPDATE GOOGLE USER (safe update)
    // ======================================================
    public async Task CreateOrUpdateGoogleUser(string userId, string username, string email)
    {
        if (db == null) return;

        var userRef = db.Child("users").Child(userId);

        // ✅ Only update these fields (won't delete others)
        var updates = new Dictionary<string, object>
        {
            { "name", username },
            { "email", email },
            { "lastLoginAt", System.DateTime.Now.ToString("o") }
        };

        await userRef.UpdateChildrenAsync(updates);

        Debug.Log("✔ Google user saved/updated to Firebase");
    }

    // ======================================================
    // SAVE PROFILE PICTURE FILE NAME
    // ======================================================
    public async Task UpdateUserProfilePicture(string userId, string fileName)
    {
        if (db == null) return;
        await db.Child("users").Child(userId).Child("profilePictureUrl").SetValueAsync(fileName);
    }

    public async Task UpdateGeneratedAvatar(string userId, string fileName)
    {
        if (db == null) return;
        await db.Child("users").Child(userId).Child("generatedPictureUrl").SetValueAsync(fileName);
    }
}

[System.Serializable]
public class UserModel
{
    public string name;
    public string createdAt;
}
