using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public interface IQuestionProvider
{
    void Initialize();
    Question GetNext(LadderDifficulty diff, HashSet<int> excludedIds);
}

[System.Serializable]
public class Question
{
    public int Id;
    public string Text;
    public string[] Choices;
    public int CorrectIndex;
    public string Explanation;

    public Question(int id, string text, string[] choices, int correctIndex, string explanation)
    {
        Id = id;
        Text = text;
        Choices = choices;
        CorrectIndex = correctIndex;
        Explanation = explanation;
    }
}

public class InMemoryQuestionProvider : IQuestionProvider
{
    private readonly Dictionary<LadderDifficulty, List<Question>> _byDiff =
        new Dictionary<LadderDifficulty, List<Question>>();

    private System.Random _rng;

    public void Initialize()
    {
        if (_rng == null) _rng = new System.Random();

        _byDiff.Clear();
    }

    public Question GetNext(LadderDifficulty diff, HashSet<int> excludedIds)
    {
        if (!_byDiff.TryGetValue(diff, out var pool) || pool == null || pool.Count == 0)
            return null;

        // Filter by excluded
        IEnumerable<Question> candidates = pool;
        if (excludedIds != null && excludedIds.Count > 0)
            candidates = candidates.Where(q => q != null && !excludedIds.Contains(q.Id));

        // Pick a random remaining, or null if none
        var list = candidates as IList<Question> ?? candidates.ToList();
        if (list.Count == 0) return null;

        int idx = _rng.Next(0, list.Count);
        return list[idx];
    }
}
