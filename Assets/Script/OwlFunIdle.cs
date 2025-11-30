using UnityEngine;

/// Fun idle animation for the owl:
/// - gentle float
/// - small rotation tilt
/// - random little "hop" every few seconds
[RequireComponent(typeof(RectTransform))]
public class OwlFunIdle : MonoBehaviour
{
    RectTransform rect;
    Vector2 startPos;

    [Header("Base Float")]
    public float floatAmplitude = 15f;   // up/down pixels
    public float floatSpeed = 2.2f;      // speed of bobbing

    [Header("Tilt / Sway")]
    public float tiltAngle = 6f;         // degrees left/right
    public float tiltSpeed = 3f;

    [Header("Random Hop")]
    public float hopHeight = 35f;        // extra jump height
    public float hopDuration = 0.35f;    // how long jump takes
    public float minHopInterval = 2f;    // random delay between hops
    public float maxHopInterval = 5f;

    [Header("Breathing Scale")]
    public float breatheAmount = 0.05f;  // 0.05 = 5% size change
    public float breatheSpeed = 1.5f;

    float hopTimer;
    float nextHopTime;
    float hopTime;
    bool isHopping;
    Vector2 hopOffset;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        ScheduleNextHop();
    }

    void ScheduleNextHop()
    {
        hopTimer = 0f;
        nextHopTime = Random.Range(minHopInterval, maxHopInterval);
    }

    void StartHop()
    {
        isHopping = true;
        hopTime = 0f;
    }

    void Update()
    {
        float t = Time.time;

        // ---------- base float & tilt ----------
        float bob = Mathf.Sin(t * floatSpeed) * floatAmplitude;
        float tilt = Mathf.Sin(t * tiltSpeed) * tiltAngle;

        // ---------- random hop ----------
        if (isHopping)
        {
            hopTime += Time.deltaTime;
            float normalized = Mathf.Clamp01(hopTime / hopDuration);
            // nice up-and-down curve (0 -> 1 -> 0)
            float curve = Mathf.Sin(normalized * Mathf.PI);
            hopOffset = new Vector2(0, curve * hopHeight);

            if (normalized >= 1f)
            {
                isHopping = false;
                hopOffset = Vector2.zero;
                ScheduleNextHop();
            }
        }
        else
        {
            hopTimer += Time.deltaTime;
            if (hopTimer >= nextHopTime)
                StartHop();
        }

        // ---------- apply position ----------
        Vector2 pos = startPos;
        pos.y += bob;
        pos += hopOffset;
        rect.anchoredPosition = pos;

        // ---------- breathing scale ----------
        float breathe = 1f + Mathf.Sin(t * breatheSpeed) * breatheAmount;
        rect.localScale = new Vector3(breathe, breathe, 1f);

        // ---------- tilt ----------
        rect.localEulerAngles = new Vector3(0, 0, tilt);
    }
}
