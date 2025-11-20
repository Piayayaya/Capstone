using System.Threading.Tasks;
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
    // CREATE USER
    // ======================================================
    public async Task CreateUser(string userId, string username)
    {
        if (db == null) return;

        var data = new UserModel
        {
            name = username,
            createdAt = System.DateTime.Now.ToString()
        };

        string json = JsonUtility.ToJson(data);
        await db.Child("users").Child(userId).SetRawJsonValueAsync(json);

        Debug.Log("✔ User saved to Firebase");
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
