using UnityEngine;
using UnityEngine.SceneManagement;

public class DifficultySelectUI : MonoBehaviour
{
    [SerializeField] string gameplaySceneName = "SmartLadder-Easy";  // the scene you're using now

    public void PickEasy() { Pick(LadderDifficulty.Easy); }
    public void PickNormal() { Pick(LadderDifficulty.Normal); }
    public void PickHard() { Pick(LadderDifficulty.Hard); }
    public void PickAdvanced() { Pick(LadderDifficulty.Advanced); }
    public void PickExpert() { Pick(LadderDifficulty.Expert); }

    void Pick(LadderDifficulty diff)
    {
        SmartLadderSession.SelectedDifficulty = diff;
        SceneManager.LoadScene(gameplaySceneName);
    }
}
