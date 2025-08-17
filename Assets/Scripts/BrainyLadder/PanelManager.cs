using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelManager : MonoBehaviour
{
    public static PanelManager Instance;  // Singleton instance

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject gameplayPanel;
    public GameObject questionPanel;
    public GameObject levelCompletePanel;
    public GameObject gameOverPanel;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Keeps the instance persistent between scenes if needed
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void ShowStartPanel()
    {
        HideAllPanels();
        startPanel.SetActive(true);
    }

    public void ShowGameplayPanel()
    {
        HideAllPanels();
        gameplayPanel.SetActive(true);
    }

    public void ShowQuestionPanel()
    {
        HideAllPanels();
        questionPanel.SetActive(true);
    }

    public void ShowLevelCompletePanel()
    {
        HideAllPanels();
        levelCompletePanel.SetActive(true);
    }

    public void ShowGameOverPanel()
    {
        HideAllPanels();
        gameOverPanel.SetActive(true);
    }

    private void HideAllPanels()
    {
        startPanel.SetActive(false);
        gameplayPanel.SetActive(false);
        questionPanel.SetActive(false);
        levelCompletePanel.SetActive(false);
        gameOverPanel.SetActive(false);
    }
}
