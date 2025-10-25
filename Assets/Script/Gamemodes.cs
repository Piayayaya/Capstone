using UnityEngine;
using UnityEngine.SceneManagement;

public class Gamemodes : MonoBehaviour
{
    [SerializeField] string sceneToLoad = "Gamemodes"; // exact scene name as shown in Project

    public void ShowGamemode()
    {
        Debug.Log("[UI] Play clicked → load " + sceneToLoad);
        SceneManager.LoadSceneAsync(sceneToLoad, LoadSceneMode.Single);
        // OR, if you prefer index: SceneManager.LoadSceneAsync(1, LoadSceneMode.Single);
    }
}
