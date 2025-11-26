using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Newtonsoft.Json;

public class SqliteQuestionProvider : IQuestionProvider
{
    private readonly Dictionary<LadderDifficulty, List<LocalQuestion>> _byDiff =
        new Dictionary<LadderDifficulty, List<LocalQuestion>>();

    private System.Random _rng;
    private const string SMARTLADDER_ID = "smartladder";

    public void Initialize()
    {
        if (_rng == null) _rng = new System.Random();
        _byDiff.Clear();

        LoadDifficulty(LadderDifficulty.Easy);
        LoadDifficulty(LadderDifficulty.Normal);
        LoadDifficulty(LadderDifficulty.Hard);
        LoadDifficulty(LadderDifficulty.Advanced);
        LoadDifficulty(LadderDifficulty.Expert);

        Debug.Log($"[SqliteQuestionProvider] Loaded pools: " +
                  string.Join(", ", _byDiff.Select(kv => $"{kv.Key}={kv.Value.Count}")));
    }

    private void LoadDifficulty(LadderDifficulty diff)
    {
        string diffKey = ToKey(diff);

        var pool = LocalDb.DB.Table<LocalQuestion>()
            .Where(q => q.gameMode_id == SMARTLADDER_ID && q.difficulty == diffKey)
            .ToList();

        _byDiff[diff] = pool ?? new List<LocalQuestion>();
    }

    public Question GetNext(LadderDifficulty diff, HashSet<int> excludedIds)
    {
        if (!_byDiff.TryGetValue(diff, out var pool) || pool == null || pool.Count == 0)
            return null;

        IEnumerable<LocalQuestion> candidates = pool;

        // Remove excluded IDs (parse id first)
        if (excludedIds != null && excludedIds.Count > 0)
            candidates = candidates.Where(q =>
            {
                int id = ParseInt(q.id);
                return q != null && !excludedIds.Contains(id);
            });

        var list = candidates as IList<LocalQuestion> ?? candidates.ToList();
        if (list.Count == 0) return null;

        int idx = _rng.Next(0, list.Count);
        var row = list[idx];

        int parsedId = ParseInt(row.id);
        int parsedCorrect = ParseInt(row.correctIndex);

        // choicesJson -> string[]
        string[] choices = new string[0];
        try
        {
            var tmp = JsonConvert.DeserializeObject<List<string>>(row.choicesJson);
            if (tmp != null) choices = tmp.ToArray();
        }
        catch
        {
            Debug.LogWarning($"[SqliteQuestionProvider] Bad choicesJson for id={parsedId}");
        }

        return new Question(
            id: parsedId,
            text: row.question,
            choices: choices,
            correctIndex: parsedCorrect,
            explanation: row.explanation ?? ""
        );
    }

    private static int ParseInt(object val)
    {
        if (val == null) return 0;
        if (val is int i) return i;

        int.TryParse(val.ToString(), out int parsed);
        return parsed;
    }

    private static string ToKey(LadderDifficulty d)
    {
        switch (d)
        {
            case LadderDifficulty.Easy: return "easy";
            case LadderDifficulty.Normal: return "normal";
            case LadderDifficulty.Hard: return "hard";
            case LadderDifficulty.Advanced: return "advanced";
            case LadderDifficulty.Expert: return "expert";
            default: return "easy";
        }
    }
}
