using UnityEngine;

/// Drifting clouds with gentle bobbing for UI (RectTransform).
/// Move horizontally and loop when they travel a certain distance.
[RequireComponent(typeof(RectTransform))]
public class CloudDrift : MonoBehaviour
{
    RectTransform rect;
    Vector2 startPos;

    [Header("Horizontal Move")]
    public bool moveRight = true;
    public float minSpeed = 20f;   // pixels per second
    public float maxSpeed = 40f;
    public float loopDistance = 800f; // how far before wrapping around

    [Header("Vertical Bobbing")]
    public float bobAmplitude = 10f;
    public float bobSpeed = 1.5f;

    float speed;
    float bobPhase;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        startPos = rect.anchoredPosition;
        speed = Random.Range(minSpeed, maxSpeed);
        bobPhase = Random.Range(0f, 10f);
    }

    void Update()
    {
        float dir = moveRight ? 1f : -1f;

        // horizontal move
        Vector2 pos = rect.anchoredPosition;
        pos.x += dir * speed * Time.deltaTime;

        // wrap around when far enough from start
        float offsetX = pos.x - startPos.x;
        if (Mathf.Abs(offsetX) > loopDistance)
        {
            pos.x = startPos.x - offsetX;   // appear on opposite side
        }

        // vertical bobbing
        float bob = Mathf.Sin(Time.time * bobSpeed + bobPhase) * bobAmplitude;
        pos.y = startPos.y + bob;

        rect.anchoredPosition = pos;
    }
}
