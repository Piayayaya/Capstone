using UnityEngine;

public class DailyQuestPanelOpener : MonoBehaviour
{
    [Header("Assign the panel GameObject (set it inactive by default)")]
    public GameObject panel;

    void Awake()
    {
        if (panel) panel.SetActive(false);   // start hidden
    }

    public void Open()
    {
        if (panel) panel.SetActive(true);
    }

    public void Close()
    {
        if (panel) panel.SetActive(false);
    }

    public void Toggle()
    {
        if (!panel) return;
        panel.SetActive(!panel.activeSelf);
    }
}
