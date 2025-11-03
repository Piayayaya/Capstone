using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoStore : MonoBehaviour
{
    [Tooltip("Scene to load when the arrow is clicked.")]
    public string sceneName = "Store";

    // Hook this up to the Button's OnClick event
    public void Go()
    {
        // Optional: safety check if scene is in Build Settings
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[ArrowLeftToDashboard] No scene name set.");
            return;
        }
        SceneManager.LoadScene(sceneName);
    }
}
