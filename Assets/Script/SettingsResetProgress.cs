using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Firebase.Auth;
using Firebase.Database;

public class SettingsResetProgress : MonoBehaviour
{
    [Header("Toggle Visuals")]
    public RectTransform knob;       // white circle/knob
    public RectTransform offAnchor;  // left position
    public RectTransform onAnchor;   // right position
    public float moveDuration = 0.15f;

    [Header("Optional UI")]
    public GameObject busyBlocker;   // optional: panel that blocks spam taps
    public TMPro.TMP_Text statusText; // optional: small text label at bottom

    bool _isBusy;

    Vector2 _offPos;
    Vector2 _onPos;

    private void Awake()
    {
        if (knob != null && offAnchor != null && onAnchor != null)
        {
            _offPos = offAnchor.anchoredPosition;
            _onPos = onAnchor.anchoredPosition;

            // start in OFF position
            knob.anchoredPosition = _offPos;
        }
    }

    /// <summary>
    /// Hook this to the Reset Progress toggle/button.
    /// Later, if you add a confirmation popup, call this
    /// from your "YES" button instead.
    /// </summary>
    public void OnClickResetProgress()
    {
        if (_isBusy) return;
        StartCoroutine(ResetRoutine());
    }

    IEnumerator ResetRoutine()
    {
        _isBusy = true;
        SetBusy(true);

        // 1) slide knob to ON side for animation
        yield return MoveKnob(_onPos);

        // 2) reset data in Firebase
        yield return ResetPlayerProgressCoroutine();

        // tiny pause so player sees it "on"
        yield return new WaitForSeconds(0.1f);

        // 3) slide knob back to OFF (because this is a one-shot action)
        yield return MoveKnob(_offPos);

        SetBusy(false);
        _isBusy = false;
    }

    IEnumerator MoveKnob(Vector2 target)
    {
        if (knob == null) yield break;

        Vector2 start = knob.anchoredPosition;
        float t = 0f;

        while (t < moveDuration)
        {
            t += Time.unscaledDeltaTime;
            float f = Mathf.Clamp01(t / moveDuration);
            knob.anchoredPosition = Vector2.Lerp(start, target, f);
            yield return null;
        }

        knob.anchoredPosition = target;
    }

    IEnumerator ResetPlayerProgressCoroutine()
    {
        var auth = FirebaseAuth.DefaultInstance;
        var user = auth.CurrentUser;

        if (user == null)
        {
            Debug.LogWarning("[SettingsResetProgress] No signed-in user, cannot reset progress.");
            SetStatus("No user signed in.");
            yield break;
        }

        string uid = user.UserId;
        var root = FirebaseDatabase.DefaultInstance.RootReference;

        // 🔁 This is based on your Firebase tree screenshot
        // /players/{uid}/coins/byMode/...
        var updates = new Dictionary<string, object>
        {
            { $"players/{uid}/coins/byMode/DailyQuests",   0 },
            { $"players/{uid}/coins/byMode/DailyRewards",  0 },
            { $"players/{uid}/coins/byMode/DragAndDrop",   0 },
            { $"players/{uid}/coins/byMode/NameTheFlag",   0 },
            { $"players/{uid}/coins/byMode/SeeItOrLoseIt", 0 },
            { $"players/{uid}/coins/byMode/SmartLadder",   0 },
            { $"players/{uid}/coins/byMode/TuneYourTongue",0 },
            { $"players/{uid}/coins/total",                0 }
        };

        var task = root.UpdateChildrenAsync(updates);

        while (!task.IsCompleted)
            yield return null;

        if (task.IsFaulted)
        {
            Debug.LogWarning("[SettingsResetProgress] Reset failed: " + task.Exception);
            SetStatus("Reset failed. Check connection.");
        }
        else
        {
            Debug.Log("[SettingsResetProgress] Progress reset to 0 in Firebase.");
            SetStatus("Progress reset!");
        }
    }

    void SetBusy(bool busy)
    {
        if (busyBlocker != null)
            busyBlocker.SetActive(busy);
    }

    void SetStatus(string msg)
    {
        if (statusText != null)
            statusText.text = msg;
    }
}
