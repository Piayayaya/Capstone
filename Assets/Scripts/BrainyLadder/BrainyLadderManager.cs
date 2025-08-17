using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    [Header("Player & Movement")]
    public GameObject player;
    public Transform[] cloudPositions;
    public Transform ladderBottomPosition;
    public Transform ladderTopPosition;
    public float moveDuration = 0.5f;
    public float climbDuration = 0.5f;

    [Header("UI Panels")]
    public GameObject startPanel;
    public GameObject gameplayPanel;
    public GameObject questionPanel;
    public GameObject levelClearPanel;
    public GameObject gameOverPanel;

    [Header("UI Elements")]
    public TMP_Text questionText;
    public Button[] answerButtons;
    public Image[] heartIcons;

    [Header("Sprites")]
    public Sprite checkMarkSprite, wrongMarkSprite;

    private int currentCloud = 0;
    private int lives = 3;
    private Question currentQuestion;
    private List<Question> questions;

    private void Start()
    {
        DatabaseManager dbManager = FindFirstObjectByType<DatabaseManager>();
        questions = dbManager.GetQuestions("BrainyLadder");
        PanelManager.Instance.ShowStartPanel();
    }

    public void StartGame()
    {
        PanelManager.Instance.ShowGameplayPanel();
        StartCoroutine(MoveToLadderBottom());
    }

    IEnumerator MoveToLadderBottom()
    {
        yield return MoveToPosition(ladderBottomPosition.position, moveDuration);
        yield return MoveToPosition(ladderTopPosition.position, climbDuration);
        yield return MoveToCloud(0);
    }

    IEnumerator MoveToCloud(int cloudIndex)
    {
        Debug.Log("Moving to cloud index: " + cloudIndex);

        Vector3 targetPosition = cloudPositions[cloudIndex].position;
        targetPosition.y += 100f; // Ensure correct foot placement
        yield return MoveToPosition(targetPosition, moveDuration);

        currentCloud = cloudIndex;
        Debug.Log("Arrived at cloud index: " + currentCloud);

        // ✅ If the player reaches the last cloud (where the chest is), complete the level
        if (cloudIndex == cloudPositions.Length - 1)
        {
            Debug.Log("🎯 Player reached the last cloud (chest)!");
            yield return new WaitForSeconds(0.5f); // Small delay before showing level clear
            PanelManager.Instance.ShowLevelCompletePanel();
        }
        else
        {
            StartCoroutine(ShowQuestionWithDelay()); // Otherwise, continue with the quiz
        }
    }

    IEnumerator MoveToPosition(Vector3 targetPosition, float duration)
    {
        Vector3 startPosition = player.transform.position;
        targetPosition.z = -1f;
        float elapsedTime = 0f;
        while (elapsedTime < duration)
        {
            player.transform.position = Vector3.Lerp(startPosition, targetPosition, elapsedTime / duration);
            elapsedTime += Time.deltaTime;
            yield return null;
        }
        player.transform.position = targetPosition;
    }

    public void AnswerQuestion(int choiceIndex)
    {
        questionPanel.SetActive(false); // Hide question panel after answering

        if (currentQuestion.CorrectAnswerIndex == choiceIndex)
        {
            ReplaceQuestionMark(currentCloud, checkMarkSprite, new Vector2(80f, 50f)); // ✅ Correct answer mark

            if (++currentCloud < cloudPositions.Length) // Allow moving to last cloud (chest)
            {
                StartCoroutine(MoveToCloud(currentCloud));
            }
            else
            {
                PanelManager.Instance.ShowLevelCompletePanel(); // If already at the last cloud (chest), complete the level
            }
        }
        else
        {
            ReplaceQuestionMark(currentCloud, wrongMarkSprite, new Vector2(60f, 60f)); // ❌ Wrong answer mark
            LoseHeart();

            if (lives > 0)
            {
                if (++currentCloud < cloudPositions.Length)
                {
                    StartCoroutine(MoveToCloud(currentCloud));
                }
                else
                {
                    PanelManager.Instance.ShowLevelCompletePanel(); // If already at last cloud (chest), complete the level
                }
            }
            else
            {
                PanelManager.Instance.ShowGameOverPanel();
                Debug.Log("🎉 Level Failed! Showing Game Over Panel...");
            }
        }
    }

    void ReplaceQuestionMark(int cloudIndex, Sprite newMark, Vector2 newSize)
    {
        Transform cloud = cloudPositions[cloudIndex];

        // Find and remove the existing question mark
        foreach (Transform child in cloud)
        {
            if (child.name == "QuestionMark")
            {
                Destroy(child.gameObject);
                break;
            }
        }

        // Add new mark
        GameObject markObject = new GameObject("Mark");
        markObject.transform.SetParent(cloud);
        markObject.transform.localPosition = new Vector3(0, 50f, 0);

        Image markImage = markObject.AddComponent<Image>();
        markImage.sprite = newMark;

        // Set the custom size
        RectTransform rectTransform = markObject.GetComponent<RectTransform>();
        rectTransform.sizeDelta = newSize;
    }

    void LoseHeart()
    {
        lives--;
        heartIcons[lives].enabled = false;
    }

    IEnumerator ShowQuestionWithDelay()
    {
        yield return new WaitForSeconds(0.5f); // Adjust delay time as needed

        int randomIndex = Random.Range(0, questions.Count);
        currentQuestion = questions[randomIndex];
        questions.RemoveAt(randomIndex);
        questionText.text = currentQuestion.Text;

        for (int i = 0; i < answerButtons.Length; i++)
        {
            answerButtons[i].GetComponentInChildren<TMP_Text>().text = currentQuestion.Answers[i];
            int choiceIndex = i;
            answerButtons[i].onClick.RemoveAllListeners();
            answerButtons[i].onClick.AddListener(() => AnswerQuestion(choiceIndex));
        }

        questionPanel.SetActive(true);
    }

}