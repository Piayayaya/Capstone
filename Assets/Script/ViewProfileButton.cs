using UnityEngine;
using UnityEngine.SceneManagement;

public class ViewProfileButton : MonoBehaviour
{
    
    [SerializeField] private string viewProfileSceneName = "View Profile";

  
    public void GoToViewProfile()
    {
        SceneManager.LoadScene(viewProfileSceneName);
    }
}
