using UnityEngine;
using UnityEngine.SceneManagement;

public class DragAndDropLoader : MonoBehaviour
{
    // Scene name must match Build Settings
    [SerializeField] private string sceneName = "DragAndDrop";

    [TextArea(6, 20)]
    public string message =
        "<align=center>DRAG AND DROP</align>\n\n" +
        "The Drag and Drop game is a fun word puzzle where you look at a picture and guess the word it shows! The letters are mixed up, and you need to drag them into the right boxes to spell the correct word. There’s a timer, so try to finish before time runs out!\n\n" +
        "• You can use one hint per game to reveal one correct letter.\n" +
        "• If you get the word before the timer ends, you’ll earn 10 coins.\n\n" +
        "Be fast, be smart, and have fun spelling the right words!";

    // Hook this to the Button → On Click()
    public void LoadDragAndDrop()
    {
        // Find the shared intro panel even if it starts inactive
        var intro = ModeIntroSimple.Instance ?? FindObjectOfType<ModeIntroSimple>(true);
        if (!intro)
        {
            Debug.LogError("[DragAndDropLoader] ModeIntroSimple not found. Loading scene directly.");
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            return;
        }

        intro.Open(message, () =>
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
        });
    }
}
