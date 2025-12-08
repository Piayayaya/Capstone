using System.Threading.Tasks;
using System.Collections.Generic;
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
#if UNITY_ANDROID && !UNITY_EDITOR
        await Task.Delay(500);
        db = FirebaseDatabase.DefaultInstance.RootReference;
        Debug.Log("DatabaseService initialized");
#else
        db = null;
        await Task.CompletedTask;
        Debug.LogWarning("DatabaseService: Firebase disabled in Editor / non-Android. Using local-only mode.");
#endif
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
    // DEVICE -> USER mapping (ONE USER PER DEVICE)
    // ======================================================

    public async Task ClaimDevice(string deviceKey, string userId)
    {
        if (db == null) return;
        await db.Child("deviceUsers").Child(deviceKey).SetValueAsync(userId);
        Debug.Log($"✔ Device claimed: {deviceKey} -> {userId}");
    }

    public async Task ReleaseDevice(string deviceKey)
    {
        if (db == null) return;
        await db.Child("deviceUsers").Child(deviceKey).RemoveValueAsync();
        Debug.Log($"✔ Device released: {deviceKey}");
    }

    public async Task<string> GetMappedUserForDevice(string deviceKey)
    {
        if (db == null) return null;

        var snap = await db.Child("deviceUsers").Child(deviceKey).GetValueAsync();
        if (!snap.Exists || snap.Value == null) return null;

        return snap.Value.ToString();
    }

    public async Task<bool> UserExists(string userId)
    {
        if (db == null) return false;

        var snap = await db.Child("users").Child(userId).GetValueAsync();
        return snap.Exists;
    }

    public async Task<string> GetUserName(string userId)
    {
        if (db == null) return null;

        var snap = await db.Child("users").Child(userId).Child("name").GetValueAsync();
        if (!snap.Exists || snap.Value == null) return null;

        return snap.Value.ToString();
    }

    // ======================================================
    // UPDATE USERNAME ONLY
    // ======================================================
    public async Task UpdateUserName(string userId, string newUsername)
    {
        if (db == null) return;
        if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(newUsername)) return;

        var updates = new Dictionary<string, object>
        {
            { "name", newUsername.Trim() },
            { "updatedAt", System.DateTime.Now.ToString("o") }
        };

        await db.Child("users").Child(userId).UpdateChildrenAsync(updates);

        Debug.Log("✔ Username updated in Firebase");
    }

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
