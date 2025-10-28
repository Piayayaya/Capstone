using UnityEngine;

public class DemoTools : MonoBehaviour
{
    // Reset only the currently selected ladder (coins, level, asked set, etc.)
    public void ResetThisDifficulty()
    {
        ProgressStore.ClearDifficulty(SmartLadderSession.SelectedDifficulty);
        Debug.Log("[DemoTools] Cleared progress for " + SmartLadderSession.SelectedDifficulty);

        // Optional: snap back to Level 1 and clear quiz state if you’re in the ladder scene
        var quiz = FindObjectOfType<SmartLadderQuiz>();
        var gameplay = FindObjectOfType<Gameplay>();
        if (quiz) quiz.ResetRun();
        if (gameplay) gameplay.SnapToLevel(1);
    }

    // Reset everything for all difficulties
    public void ResetAll()
    {
        ProgressStore.ClearAll();
        Debug.Log("[DemoTools] Cleared ALL progress.");

        var quiz = FindObjectOfType<SmartLadderQuiz>();
        var gameplay = FindObjectOfType<Gameplay>();
        if (quiz) quiz.ResetRun();
        if (gameplay) gameplay.SnapToLevel(1);
    }
}
