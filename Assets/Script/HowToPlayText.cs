using UnityEngine;
using TMPro;

/// Attach this to your "Learn How to PLay Text" object.
/// Drag the white box (Box For Learn How to play) into targetBox in the Inspector.
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TMP_Text))]
public class HowToPlayText : MonoBehaviour
{
    [Header("Where should the text live?")]
    public RectTransform targetBox;          // drag "Box For Learn How to play" here

    [Header("Padding inside the box")]
    public float horizontalPadding = 40f;
    public float verticalPadding = 40f;

    RectTransform rect;
    TMP_Text tmp;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        tmp = GetComponent<TMP_Text>();

        SetupLayout();
        SetupText();
    }

    void SetupLayout()
    {
        if (targetBox == null)
        {
            Debug.LogWarning("HowToPlayText: targetBox is not assigned.");
            return;
        }

        // Make this text a child of the target box (visually still same place)
        rect.SetParent(targetBox, false);

        // Stretch to fill the box with padding
        rect.anchorMin = new Vector2(0f, 0f);   // bottom-left
        rect.anchorMax = new Vector2(1f, 1f);   // top-right
        rect.pivot = new Vector2(0.5f, 1f); // top-center

        rect.offsetMin = new Vector2(horizontalPadding, verticalPadding);   // left, bottom
        rect.offsetMax = new Vector2(-horizontalPadding, -verticalPadding); // right, top
    }

    void SetupText()
    {
        tmp.enableWordWrapping = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.richText = true;

        tmp.text =
            "Welcome to BrainyMe!\n" +
            "BrainyMe is a fun learning game where you answer questions, earn coins, " +
            "and become smarter every day. Choose any game mode you like and try to get the highest score!\n\n" +

            "Smart Ladder – Answer questions correctly to climb up the ladder. The higher you go, " +
            "the harder (and more exciting) the questions become.\n\n" +

            "Name the Flag – Look at the flag and guess the correct country. Use your clues " +
            "and try to collect as many flags as you can.\n\n" +

            "Drag & Drop – Drag the correct answer or picture into the right box. " +
            "Match them all before time runs out.\n\n" +

            "Tune Your Tongue – Listen, read, and then say the word or phrase. Speak clearly into the microphone " +
            "and see if the owl approves your pronunciation.\n\n" +

            "See It or Lose It – Look carefully and spot the differences between the pictures before they disappear. " +
            "Don’t miss any tiny details!\n\n" +

            "Finish rounds to earn coins, unlock cute characters, and complete achievements. " +
            "Play every day, finish daily quests, and let BrainyMe help you learn while having fun!";
    }
}
