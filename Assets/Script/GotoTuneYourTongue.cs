using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoTuneYourTongue : MonoBehaviour
{
    [Tooltip("Scene to load when the button is clicked.")]
    public string sceneName = "TuneYourTongue";

    [TextArea(6, 20)]
    public string message =
        "<align=center>TUNE YOUR TONGUE</align>\n\n" +
        "The Tune Your Tongue game is a fun way to practice saying words correctly. " +
        "You will see a word or picture, and you need to say the word into the microphone. " +
        "There is a timer, so speak before the time runs out. " +
        "If you say the word correctly, you will earn 10 coins. " +
        "This game helps you learn how to pronounce words the right way while having fun.";

    public void Go()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GotoTuneYourTongue] No scene name set.");
            return;
        }

        // Get the shared intro panel even if it starts disabled
        var intro = ModeIntroSimple.Instance ?? FindObjectOfType<ModeIntroSimple>(true);
        if (!intro)
        {
            Debug.LogError("[GotoTuneYourTongue] ModeIntroSimple not found. Loading scene directly.");
            SceneManager.LoadScene(sceneName);
            return;
        }

        intro.Open(message, () =>
        {
            SceneManager.LoadScene(sceneName);
        });
    }
}
