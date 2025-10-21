using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnClick : MonoBehaviour
{
    public string sceneName = "Background_NameTheFlag";
    public float delay = 0f;

    public void LoadScene()
    {
        if (delay <= 0f) SceneManager.LoadScene(sceneName);
        else StartCoroutine(WaitAndLoad());
    }

    System.Collections.IEnumerator WaitAndLoad()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}