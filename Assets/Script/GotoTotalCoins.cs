using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GotoTotalCoins : MonoBehaviour
{
    [SerializeField] private string viewProfileSceneName = "TotalCoins";
    public void GoToTotalCoins()
    {
        SceneManager.LoadScene(viewProfileSceneName);
    }
}
