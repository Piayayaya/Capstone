using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class LadderUIBuilder : MonoBehaviour
{
    [Header("UI References")]
    public GameObject levelPrefab;         // Prefab for each level
    public RectTransform contentPanel;     // Content inside the Scroll View

    [Header("Level Settings")]
    public int totalLevels = 20;
    public float verticalSpacing = 300f;
    public float horizontalOffset = 150f;
    public float ladderLeftAngle = -25f;
    public float ladderRightAngle = 25f;

    [Header("Optional: Character Anchor Points")]
    public List<RectTransform> characterSpots = new List<RectTransform>();

    void Start()
    {
        GenerateLevels();
    }

    void GenerateLevels()
    {
        for (int i = totalLevels - 1; i >= 0; i--)
        {
            // Instantiate prefab and parent it under content panel
            GameObject level = Instantiate(levelPrefab, contentPanel);
            RectTransform levelRT = level.GetComponent<RectTransform>();

            // Calculate position (grows downward, visually upward in level number)
            float y = -(totalLevels - 1 - i) * verticalSpacing;
            float x = (i % 2 == 0) ? -horizontalOffset : horizontalOffset;

            levelRT.anchoredPosition = new Vector2(x, y);

            // Set level number text if available
            var text = level.transform.Find("LevelNumberText")?.GetComponent<Text>();
            if (text != null) text.text = $"Level {i + 1}";

            // Set ladder angle
            var ladder = level.transform.Find("LadderImage");
            if (ladder != null)
            {
                float angle = (i % 2 == 0) ? ladderRightAngle : ladderLeftAngle;
                ladder.rotation = Quaternion.Euler(0, 0, angle);
            }

            // Store character spot if available
            var holder = level.transform.Find("CharacterHolder")?.GetComponent<RectTransform>();
            if (holder != null)
                characterSpots.Add(holder);
        }

        // Resize content height to fit all levels
        float totalHeight = totalLevels * verticalSpacing + 300f;
        contentPanel.sizeDelta = new Vector2(contentPanel.sizeDelta.x, totalHeight);
    }
}
