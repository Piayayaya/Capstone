using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]  // optional: auto-wires if this is on a Button
public class GotoCreateAccount : MonoBehaviour
{
    [Tooltip("Exact scene name in Build Settings > Scenes In Build")]
    [SerializeField] private string sceneName = "CreateAccount";

    private void Awake()
    {
        // If this component is on a Button, auto-hook its click
        var btn = GetComponent<Button>();
        if (btn) btn.onClick.AddListener(Go);
    }

    // You can also call this directly from the Button's OnClick in the Inspector
    public void Go()
    {
        if (string.IsNullOrEmpty(sceneName))
        {
            Debug.LogError("[GotoCreateAccount] No scene name set.");
            return;
        }

        if (!Application.CanStreamedLevelBeLoaded(sceneName))
        {
            Debug.LogError($"[GotoCreateAccount] Scene '{sceneName}' is not in Build Settings.");
            return;
        }

        SceneManager.LoadScene(sceneName, LoadSceneMode.Single);
    }
}
