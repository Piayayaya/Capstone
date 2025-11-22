using UnityEngine;
using UnityEngine.UI;

public class InputFixer : MonoBehaviour
{
    void Start()
    {
        var images = FindObjectsOfType<Image>(true);
        foreach (var img in images)
        {
            // if it is not a button, don’t block clicks
            if (img.GetComponent<Button>() == null)
                img.raycastTarget = false;
        }
        Debug.Log("[InputFixer] Disabled raycastTarget on non-button Images.");
    }
}
