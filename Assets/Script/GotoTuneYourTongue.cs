using UnityEngine;
using UnityEngine.SceneManagement;


public class GotoTuneYourTongue : MonoBehaviour 
{
    [Tooltip("Scene to load when the arrow is clicked.")]
    public string sceneName = "TuneYourTongue";

    public void Go()
    {
        // Optional: safety check if scene is in Build Settings
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[TuneYourTongue] No scene name set.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}

