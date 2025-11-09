using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoSeeItOrLoseIt : MonoBehaviour
{
    // If you ever rename the scene, change it here (or expose it in Inspector).
    [SerializeField] private string sceneName = "SeeItorLoseIt";

    [TextArea(6, 20)]
    public string message =
        "<align=center>SEE IT OR LOSE IT</align>\n\n" +
        "The See it or Lose it game is a fun game where you look at two pictures and try to find the thing that is different. " +
        "Each game has 3 different things to find, and there is a timer, so you need to be quick. " +
        "If you find all the differences before time runs out, you will earn 10 coins. " +
        "This game helps you practice looking carefully and spotting differences while having fun.";

    // Hook this to the Button's OnClick()
    public void LoadScene()
    {
        // Find the shared intro panel even if it starts inactive
        var intro = ModeIntroSimple.Instance ?? FindObjectOfType<ModeIntroSimple>(true);

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[GotoSeeItOrLoseIt] Scene '{sceneName}' is not in Build Settings.");
            return;
        }

        if (!intro)
        {
            Debug.LogWarning("[GotoSeeItOrLoseIt] ModeIntroSimple not found. Loading scene directly.");
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
            return;
        }

        intro.Open(message, () =>
        {
            SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
        });
    }
}
