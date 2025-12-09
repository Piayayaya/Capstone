using System.Text;
using TMPro;
using UnityEngine;

public class NotificationController : MonoBehaviour
{
    [Header("UI")]
    public TMP_Text notificationText;   // drag your "Notification Text" TMP here in Inspector

    private void OnEnable()
    {
        Refresh();

        if (NotificationService.Instance != null)
            NotificationService.Instance.OnLogChanged += Refresh;
    }

    private void OnDisable()
    {
        if (NotificationService.Instance != null)
            NotificationService.Instance.OnLogChanged -= Refresh;
    }

    // Optional helper if you ever want to log directly from UI:
    public void Log(string message)
    {
        if (NotificationService.Instance != null)
        {
            NotificationService.Instance.Add(message);
        }
    }

    private void Refresh()
    {
        if (notificationText == null) return;

        if (NotificationService.Instance == null)
        {
            notificationText.text = "No notifications yet.";
            return;
        }

        var list = NotificationService.Instance.Entries;
        if (list == null || list.Count == 0)
        {
            notificationText.text = "No notifications yet.";
            return;
        }

        var sb = new StringBuilder();

        for (int i = 0; i < list.Count; i++)
        {
            var e = list[i];
            sb.AppendLine(e.createdAt);      // time
            sb.Append("• ");
            sb.AppendLine(e.message);        // message text

            if (i < list.Count - 1)
                sb.AppendLine();            // extra space between entries
        }

        notificationText.text = sb.ToString();
    }
}
