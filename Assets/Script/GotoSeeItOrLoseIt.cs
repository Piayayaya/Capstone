using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoSeeItOrLoseIt : MonoBehaviour
{
    // If you ever rename the scene, change it here (or expose it in Inspector).
    [SerializeField] private string sceneName = "SeeItorLoseIt";

    // Hook this to the Button's OnClick()
    public void LoadScene()
    {
        // Optional: guard if scene not in Build Settings
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        }
        else
        {
            Debug.LogError($"Scene '{sceneName}' is not in Build Settings.");
        }
    }
}
