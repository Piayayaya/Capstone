using UnityEngine;
using UnityEngine.SceneManagement;

public class DragAndDropLoader : MonoBehaviour
{
    // Scene name must match the asset exactly (case-sensitive on some platforms)
    [SerializeField] private string sceneName = "DragAndDrop";

    // Hook this to the Button → On Click()
    public void LoadDragAndDrop()
    {
        Debug.Log("[DragAndDropLoader] Loading: " + sceneName);
        SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
    }
}
