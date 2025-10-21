using UnityEngine;
using UnityEngine.SceneManagement;

public enum LadderDifficulty { Easy, Normal, Hard, Advanced, Expert }

public static class SmartLadderSession
{
    public static LadderDifficulty SelectedDifficulty = LadderDifficulty.Easy;
}

public class DifficultySelectUi : MonoBehaviour
{
    [SerializeField] string gameplaySceneName = "SmartLadder-Easy"; // your gameplay scene

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
