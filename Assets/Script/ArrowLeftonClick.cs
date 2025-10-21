using UnityEngine;
using UnityEngine.SceneManagement;

public class ArrowLeftonClick : MonoBehaviour
{
    [Tooltip("Scene to load when the left arrow is clicked.")]
    public string sceneName = "Gamemodes";

    public void LoadScene()
    {
        if (!string.IsNullOrEmpty(sceneName))
            SceneManager.LoadScene(sceneName);
    }
}