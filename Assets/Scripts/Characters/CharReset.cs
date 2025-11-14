using UnityEngine;

public class CharReset : MonoBehaviour
{
    [ContextMenu("Reset Character Data")]
    public void ResetCharacters()
    {
        PlayerPrefs.DeleteKey("OwnedChars");
        PlayerPrefs.DeleteKey("EquippedChar");
        PlayerPrefs.Save();
        Debug.Log("✅ Character data reset! (OwnedChars & EquippedChar cleared)");
    }
}
