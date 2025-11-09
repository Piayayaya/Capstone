using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoNameTheFlagSimple : MonoBehaviour
{
    // EXACT scene names that belong to this mode (add/remove as needed)
    [SerializeField] private string[] scenes = { "Background_NameTheFlag", "JapanBackground" };
    private const string LastSceneKey = "NameTheFlag_LastScene";

    [TextArea(6, 16)]
    public string message =
"NAME THE FLAG\n\n" +
"The Name the Flag game is a super fun way to learn about different countries around the world! Before each question starts, you’ll see a clue or picture in the background that gives you a hint about the country — it could be something like a famous place, food, or animal from that country. Then, a flag will appear on the screen, and you have to guess which country it belongs to before the timer runs out!\n\n" +
"Earn coins for correct answers:\n" +
"10 coins – first try\n" +
"5 coins – second try\n" +
"3 coins – last try\n\n" +
"Look carefully at the clue, think about the country, and tap your answer. The goal is to earn as many coins as you can and see how many flags you can name correctly — get ready to play, learn, and explore the world!";

    public void Go()
    {
        if (scenes == null || scenes.Length == 0)
        {
            Debug.LogError("[GotoNameTheFlagSimple] No scenes listed.");
            return;
        }

        // Get the shared panel even if it starts disabled
        var intro = ModeIntroSimple.Instance ?? FindObjectOfType<ModeIntroSimple>(true);
        if (!intro)
        {
            Debug.LogError("[GotoNameTheFlagSimple] ModeIntroSimple not found in scene.");
            return;
        }

        // Pick a scene (avoid instant repeat if there are 2+)
        var options = new List<string>(scenes);
        string last = PlayerPrefs.GetString(LastSceneKey, "");
        if (options.Count > 1 && !string.IsNullOrEmpty(last))
            options.Remove(last);

        string pick = options[Random.Range(0, options.Count)];
        PlayerPrefs.SetString(LastSceneKey, pick);
        PlayerPrefs.Save();

        Debug.Log($"[GotoNameTheFlagSimple] Opening intro; next scene: {pick}");
        intro.Open(message, () =>
        {
            SceneManager.LoadScene(pick);
        });
    }
}
