// RegisterUI.cs
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class RegisterUI : MonoBehaviour
{
    public TMP_InputField nameInput;
    public string nextScene = "Profile Scene";

    public void OnClickCreate()
    {
        if (ProfileService.Instance == null) { Debug.LogError("ProfileService not found in scene."); return; }
        if (!nameInput) { Debug.LogError("RegisterUI.nameInput not assigned."); return; }

        ProfileService.Instance.SetName(nameInput.text);
        SceneManager.LoadScene(nextScene);
    }
}
