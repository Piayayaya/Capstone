using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NTF_PlayAgainRouter : MonoBehaviour
{
    [Header("Scenes")]
    public string yesScene = "Background_NameTheFlag";
    public string noScene = "Gamemodes";

    [Header("Optional fade (if you have it)")]
    public CanvasGroupFader panel;   // your PLAYAGAIN CanvasGroupFader
    public float fadeOutDelay = 0.15f;

    // Hook to YES button
    public void OnYes() { StartCoroutine(Load(yesScene)); }

    // Hook to NO button
    public void OnNo() { StartCoroutine(Load(noScene)); }

    IEnumerator Load(string sceneName)
    {
        if (panel) panel.Hide();                // nice fade-out if available
        if (fadeOutDelay > 0f)
            yield return new WaitForSecondsRealtime(fadeOutDelay);

        if (string.IsNullOrWhiteSpace(sceneName))
        {
            Debug.LogError("[NTF_PlayAgainRouter] Scene name is empty.");
            yield break;
        }

        SceneManager.LoadScene(sceneName);
    }
}
