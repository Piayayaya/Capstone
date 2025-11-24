using UnityEngine;
using UnityEngine.SceneManagement;

public class GoToEditProfile : MonoBehaviour
{
    [SerializeField] private string editProfileSceneName = "EditProfile";
    // use EXACT scene name

    public void OpenEditProfile()
    {
        SceneManager.LoadScene(editProfileSceneName);
    }
}
