using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PlayerFocus : MonoBehaviour
{
    public ScrollRect scroll;            // your Scroll View
    public RectTransform content;        // Scroll View -> Viewport -> Content
    public RectTransform player;         // the player (MUST be under Content)

    [Tooltip("Call once automatically after layout settles.")]
    public bool runOnStart = true;

    void Start()
    {
        if (runOnStart) StartCoroutine(CoCenterNextFrame());
    }

    public void CenterNow()
    {
        if (!scroll || !content || !player || !scroll.viewport) return;

        // Player center in Content-local space
        Vector3 playerCenterWorld = player.TransformPoint(player.rect.center);
        float targetY = content.InverseTransformPoint(playerCenterWorld).y;

        // Viewport center in Content-local space
        var viewport = (RectTransform)scroll.viewport;
        Vector3[] vp = new Vector3[4];
        viewport.GetWorldCorners(vp);
        Vector3 vpCenterWorld = (vp[0] + vp[2]) * 0.5f;
        float viewY = content.InverseTransformPoint(vpCenterWorld).y;

        // Shift content so player aligns with viewport center
        float delta = targetY - viewY;
        Vector2 pos = content.anchoredPosition;
        pos.y += delta;

        // Clamp within scrollable range
        float span = Mathf.Max(0f, content.rect.height - viewport.rect.height);
        pos.y = Mathf.Clamp(pos.y, 0f, span);

        content.anchoredPosition = pos;
        scroll.velocity = Vector2.zero;
    }

    public IEnumerator CoCenterNextFrame()
    {
        // Wait 1–2 frames so size/positions (and any content-resize) finish first
        yield return null;
        Canvas.ForceUpdateCanvases();
        yield return null; // second frame helps with ScrollRect layouts
        Canvas.ForceUpdateCanvases();

        CenterNow();
    }
}
