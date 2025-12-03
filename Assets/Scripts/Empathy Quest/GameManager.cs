using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class QuizManager : MonoBehaviour
{
    [Header("Question Panels")]
    public GameObject[] questionPanels;   // Assign your 5 question GameObjects
    private int currentQuestionIndex = 0;

    [Header("Feedback Panels")]
    public GameObject correctPanel;
    public GameObject wrongPanel;

    [Header("End Panel")]
    public GameObject gameCompletePanel;

    [Header("Timing")]
    public float feedbackTime = 2f;
    public float endTime = 2f;

    private void Start()
    {
        ShowQuestion(0);
    }

    public void AnswerButtonPressed(bool isCorrect)
    {
        // Show correct/wrong feedback
        if (isCorrect)
        {
            correctPanel.SetActive(true);
            StartCoroutine(NextQuestionAfterDelay(correctPanel));
        }
        else
        {
            wrongPanel.SetActive(true);
            StartCoroutine(NextQuestionAfterDelay(wrongPanel));
        }
    }

    private IEnumerator NextQuestionAfterDelay(GameObject panel)
    {
        yield return new WaitForSeconds(feedbackTime);
        panel.SetActive(false);

        // Hide current question
        questionPanels[currentQuestionIndex].SetActive(false);

        currentQuestionIndex++;

        // If more questions → show next
        if (currentQuestionIndex < questionPanels.Length)
        {
            ShowQuestion(currentQuestionIndex);
        }
        else
        {
            // No more questions → Show Game Complete Panel
            StartCoroutine(ShowGameCompleteAndExit());
        }
    }

    private IEnumerator ShowGameCompleteAndExit()
    {
        gameCompletePanel.SetActive(true);
        yield return new WaitForSeconds(endTime);

        // Load Gamemodes scene
        SceneManager.LoadScene("Gamemodes");
    }

    private void ShowQuestion(int index)
    {
        // Turn all off (just to be safe)
        foreach (GameObject q in questionPanels)
            q.SetActive(false);

        questionPanels[index].SetActive(true);
    }
}
