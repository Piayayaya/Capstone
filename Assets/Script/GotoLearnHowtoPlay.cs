using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoLearnHowtoPlay : MonoBehaviour
{
    [SerializeField] private string LearnHowtoPlaySceneName = "LearnHowtoPlay";
    // use EXACT scene name

    public void OpenLearnHowtoPlay()
    {
        SceneManager.LoadScene(LearnHowtoPlaySceneName);
    }
}
