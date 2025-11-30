using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoLearnHowtoPurchase : MonoBehaviour
{
    [SerializeField] private string LearnHowtoPurchaseSceneName = "LearnHowtoPurchase";
    // use EXACT scene name

    public void OpenLearnHowtoPurchase()
    {
        SceneManager.LoadScene(LearnHowtoPurchaseSceneName);
    }
}
