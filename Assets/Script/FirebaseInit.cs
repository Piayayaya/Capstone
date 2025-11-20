using UnityEngine;
using Firebase;
using Firebase.Extensions;

public class FirebaseInit : MonoBehaviour
{
    public static bool IsReady = false;

    private void Start()
    {
        FirebaseApp.CheckAndFixDependenciesAsync().ContinueWithOnMainThread(task =>
        {
            if (task.Result == DependencyStatus.Available)
            {
                Debug.Log("🔥 Firebase Ready!");

                // IMPORTANT — set Database URL BEFORE using DatabaseService
                FirebaseApp app = FirebaseApp.DefaultInstance;
                app.Options.DatabaseUrl = new System.Uri(
                    "https://brainyme-firebase-default-rtdb.asia-southeast1.firebasedatabase.app/"
                );

                IsReady = true;
            }
            else
            {
                Debug.LogError("Firebase init failed: " + task.Result);
            }
        });
    }
}
