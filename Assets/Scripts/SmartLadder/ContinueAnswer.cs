using UnityEngine;

public class ShowWhenQuestionHidden : MonoBehaviour
{
    public SmartLadderQuiz quiz;   // drag your quiz
    public GameObject target;      // drag the Continue button object

    void LateUpdate()
    {
        if (!quiz || !target) return;

        bool questionOpen = quiz.questionPanel && quiz.questionPanel.activeSelf;
        bool explanationOpen = quiz.explanationPanel && quiz.explanationPanel.activeSelf;

        // Show only if:
        //  - a question exists,
        //  - both panels are closed,
        //  - the user manually closed the question (✕)
        bool shouldShow = quiz.HasCurrentQuestion
                          && !questionOpen
                          && !explanationOpen
                          && quiz.ReadyForManualContinue;

        if (target.activeSelf != shouldShow)
            target.SetActive(shouldShow);
    }
}