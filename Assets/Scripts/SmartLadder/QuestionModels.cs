using UnityEngine;

[System.Serializable]
public class Question
{
    public int Id;                    // stable id (reuse in SQLite/Firebase later)
    public string Text;
    public string[] Choices;          // length = 3
    public int CorrectIndex;          // 0..2
    public string Explanation;
    public LadderDifficulty Difficulty;
}

public interface IQuestionProvider
{
    void Initialize();
    // Prefer returning a question that hasn't been asked this run.
    Question GetNext(LadderDifficulty difficulty, System.Collections.Generic.HashSet<int> alreadyAskedIds);
}
