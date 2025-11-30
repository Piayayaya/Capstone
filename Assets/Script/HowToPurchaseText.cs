using UnityEngine;
using TMPro;

/// Attach this to your "How to Purchase" Text (TMP) object.
/// Drag the white box/panel where the text should live into targetBox.
[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(TMP_Text))]
public class HowToPurchaseText : MonoBehaviour
{
    [Header("Where should the text be placed?")]
    public RectTransform targetBox;          // e.g. "Box For How To Purchase"

    [Header("Padding inside the box")]
    public float horizontalPadding = 40f;
    public float verticalPadding = 40f;

    private RectTransform rect;
    private TMP_Text tmp;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        tmp = GetComponent<TMP_Text>();

        SetupLayout();
        SetupText();
    }

    private void SetupLayout()
    {
        if (targetBox == null)
        {
            Debug.LogWarning("HowToPurchaseText: targetBox is not assigned.");
            return;
        }

        // Make this text a child of the box (no world-position change)
        rect.SetParent(targetBox, false);

        // Stretch to fill the box with padding
        rect.anchorMin = new Vector2(0f, 0f);   // bottom-left
        rect.anchorMax = new Vector2(1f, 1f);   // top-right
        rect.pivot = new Vector2(0.5f, 1f); // top-center

        rect.offsetMin = new Vector2(horizontalPadding, verticalPadding);   // left, bottom
        rect.offsetMax = new Vector2(-horizontalPadding, -verticalPadding); // right, top
    }

    private void SetupText()
    {
        tmp.enableWordWrapping = true;
        tmp.alignment = TextAlignmentOptions.TopLeft;
        tmp.richText = true;

        tmp.text =
            "Using Coins\n" +
            "• Earn coins by playing game modes, completing daily quests, claiming daily rewards, and finishing achievements.\n" +
            "• Open the Store and go to the Characters section.\n" +
            "• Tap a character card to see its price.\n" +
            "• If you have enough coins, press the Buy button and confirm. Your coins will be deducted and the character will be unlocked for your account.\n\n" +

            "Equipping Characters\n" +
            "• After buying a character, open your Character Inventory.\n" +
            "• Select the character you want and tap Equip to use it in the game.\n\n" +

            "Subscriptions & Online Purchases\n" +
            "• Some items, like subscriptions, use real money and require an internet connection.\n" +
            "• When you tap Buy on a subscription, the game will open your device’s secure payment screen (such as Google Play billing).\n" +
            "• Ask a parent or guardian for permission before making any online purchase.\n\n" +

            "Tip: Play every day to earn more coins, unlock new characters faster, and enjoy all the BrainyMe store items!";
    }
}
