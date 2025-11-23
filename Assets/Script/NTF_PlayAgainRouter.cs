using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class NTF_PlayAgainRouter : MonoBehaviour
{
    [Header("YES Scenes (shuffle, no repeat)")]
    public string[] yesScenes = { "Background_NameTheFlag", "JapanBackground" };

    [Header("NO Scene")]
    public string noScene = "Gamemodes";

    [Header("Optional fade (if you have it)")]
    public CanvasGroupFader panel;   // your PLAYAGAIN CanvasGroupFader
    public float fadeOutDelay = 0.15f;

    private const string LastYesSceneKey = "NTF_LAST_YES_SCENE";

    // Hook to YES button
    public void OnYes()
    {
        string next = PickNextYesScene();
        StartCoroutine(Load(next));
    }

    // Hook to NO button
    public void OnNo()
    {
        StartCoroutine(Load(noScene));
    }

    private string PickNextYesScene()
    {
        if (yesScenes == null || yesScenes.Length == 0)
        {
            Debug.LogError("[NTF_PlayAgainRouter] yesScenes is empty.");
            return SceneManager.GetActiveScene().name;
        }

        // If only one scene configured, just use it
        if (yesScenes.Length == 1)
            return yesScenes[0];

        string last = PlayerPrefs.GetString(LastYesSceneKey, "");

        // Collect candidates excluding last
        var candidates = new System.Collections.Generic.List<string>();
        foreach (var s in yesScenes)
        {
            if (!string.IsNullOrWhiteSpace(s) && s != last)
                candidates.Add(s);
        }

        // If all were excluded (edge case), fallback to full list
        if (candidates.Count == 0)
            candidates.AddRange(yesScenes);

        string chosen = candidates[Random.Range(0, candidates.Count)];

        PlayerPrefs.SetString(LastYesSceneKey, chosen);
        PlayerPrefs.Save();

        return chosen;
    }

    IEnumerator Load(string sceneName)
    {
        if (panel) panel.Hide();
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
