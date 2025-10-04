using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNav : MonoBehaviour
{
    [SerializeField] string sceneName = "SmartLadder-DifficultySelection";
    public void Go() => SceneManager.LoadScene(sceneName);
}
