using UnityEngine;
using UnityEngine.SceneManagement;


public class GotoSmartLadder : MonoBehaviour 
{
    [Tooltip("Scene to load when the arrow is clicked.")]
    public string sceneName = "SmartLadder-DifficultySelection";

    public void Go()
    {
        // Optional: safety check if scene is in Build Settings
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GotoNameTheFlag] No scene name set.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}

