using UnityEngine;
using UnityEditor;
using TMPro;

public class BangersAssign : MonoBehaviour
{
    [MenuItem("Tools/TMP/Assign Bangers Font To Missing TMP Text")]
    public static void AssignFont()
    {
        // CHANGE THIS to the location of your Bangers SDF
        TMP_FontAsset bangersFont = AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(
            "Assets/TextMesh Pro/Fonts/Bangers SDF.asset"
        );

        if (bangersFont == null)
        {
            Debug.LogError("Bangers SDF font not found. Check the path!");
            return;
        }

        int fixedCount = 0;

        // Scan ALL TextMeshProUGUI components in project
        string[] guids = AssetDatabase.FindAssets("t:Prefab t:GameObject");

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(path);

            if (prefab == null) continue;

            TextMeshProUGUI[] texts = prefab.GetComponentsInChildren<TextMeshProUGUI>(true);

            foreach (var text in texts)
            {
                if (text.font == null) // missing!
                {
                    text.font = bangersFont;
                    EditorUtility.SetDirty(text);
                    fixedCount++;
                }
            }
        }

        Debug.Log("Assigned Bangers font to " + fixedCount + " TMP components.");
        AssetDatabase.SaveAssets();
    }
}
