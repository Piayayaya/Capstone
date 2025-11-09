using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoSmartLadder : MonoBehaviour
{
    public string sceneName = "SmartLadder-DifficultySelection";

    [TextArea(6, 20)]
    public string message =
        "<align=center>SMART LADDER</align>\n\n" +
        "The Smart Ladder game is a fun and exciting quiz game where you climb up levels by answering questions! There are five levels to play — Easy, Normal, Hard, Advance, and Expert. Each level has more questions to answer: 20 in Easy, 30 in Normal, 50 in Hard, 70 in Advance, and 100 in Expert. To move to the next level, you need to finish all the questions in your current one. Each question has a timer, so you have to think and answer before time runs out! You can earn coins depending on how many tries it takes to get the right answer — 10 coins if you get it right on your first try, 5 coins on your second, and 3 coins on your last try. The goal is to answer as many questions correctly and climb all the way to the top of the Smart Ladder!";

    public void Go()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GotoSmartLadder] No scene name set.");
            return;
        }

        var intro = ModeIntroSimple.Instance ?? FindObjectOfType<ModeIntroSimple>(true);
        if (!intro)
        {
            Debug.LogError("[GotoSmartLadder] ModeIntroSimple not found. Loading scene directly.");
            SceneManager.LoadScene(sceneName);
            return;
        }

        intro.Open(message, () => SceneManager.LoadScene(sceneName));
    }
}
