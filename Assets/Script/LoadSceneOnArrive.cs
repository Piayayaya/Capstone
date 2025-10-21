using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnArrive : MonoBehaviour
{
    [Tooltip("Scene to load when UIMoveToTarget.onArrived fires")]
    public string sceneName = "Philippines_NameTheFlag";
    public float delay = 0.25f;

    public void Go()
    {
        StartCoroutine(Co());
    }

    System.Collections.IEnumerator Co()
    {
        yield return new WaitForSeconds(delay);
        SceneManager.LoadScene(sceneName);
    }
}
