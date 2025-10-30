using UnityEngine;
using UnityEngine.SceneManagement;

public class SeeItButtons : MonoBehaviour
{
    // YES – restart this mini-game scene
    public void ReloadScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    // NO – go back to your hub (change "Gamemodes" if your scene name differs)
    public void GoToGamemodes()
    {
        SceneManager.LoadScene("Gamemodes");
    }

    // Optional – just hide the panel (if you want to keep playing)
    public GameObject panel;
    public void HidePanel()
    {
        if (panel) panel.SetActive(false);
    }
}
