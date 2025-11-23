using UnityEngine;
using UnityEngine.SceneManagement;

public class HelpButton : MonoBehaviour
{
    public void GoToHelp()
    {
        Debug.Log("Loading scene: Help");
        SceneManager.LoadScene("Help");
    }
}
