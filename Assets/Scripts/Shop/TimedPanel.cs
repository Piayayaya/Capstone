using UnityEngine;

public class TimedPanel : MonoBehaviour
{
    public GameObject panel;      // drag your panel here in the Inspector
    public float showTime = 2f;   // seconds to stay visible (2 or 3)

    public void ShowPanel()
    {
        panel.SetActive(true);
        CancelInvoke(nameof(HidePanel));        // reset if called again
        Invoke(nameof(HidePanel), showTime);    // hide after showTime seconds
    }

    void HidePanel()
    {
        panel.SetActive(false);
    }
}
