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

        // TODO: Replace these with your real data. IDs must be UNIQUE per difficulty.
        _byDiff[LadderDifficulty.Easy] = new List<Question>
        {
            new Question(2001, "What is 2 + 3?", new[] {"4", "5", "6"}, 1, "2 + 3 = 5."),
            new Question(2002, "Which animal says \"meow\"?", new[] {"Dog", "Cat", "Cow"}, 1, "Cats meow; dogs bark, cows moo."),
            new Question(2003, "Which day comes after Monday?", new[] {"Sunday", "Tuesday", "Friday"}, 1, "The order is Monday → Tuesday."),
            new Question(2004, "How many sides does a triangle have?", new[] {"3", "4", "5"}, 0, "A triangle has 3 sides."),
            new Question(2005, "What color do you get by mixing red and blue?", new[] {"Purple", "Green", "Orange"}, 0, "Red + blue = purple."),
            new Question(2006, "Which number is the smallest?", new[] {"7", "2", "9"}, 1, "2 is smaller than 7 and 9."),
            new Question(2007, "What is the first letter of the word 'Apple'?", new[] {"A", "P", "L"}, 0, "Apple starts with A."),
            new Question(2008, "Which one is a fruit?", new[] {"Carrot", "Banana", "Broccoli"}, 1, "Banana is a fruit; the others are vegetables."),
            new Question(2009, "How many legs does a spider have?", new[] {"6", "8", "10"}, 1, "Spiders have 8 legs."),
            new Question(2010, "What is 10 − 4?", new[] {"5", "6", "7"}, 1, "10 minus 4 equals 6."),
            new Question(2011, "Which shape is round?", new[] {"Square", "Circle", "Triangle"}, 1, "A circle is round."),
            new Question(2012, "Which is a source of light at night in the sky?", new[] {"Moon", "Tree", "Rock"}, 0, "The Moon reflects sunlight and lights the night sky."),
            new Question(2013, "What do we breathe in to live?", new[] {"Oxygen", "Carbon dioxide", "Helium"}, 0, "We breathe in oxygen."),
            new Question(2014, "Which season is the coldest (in many places)?", new[] {"Summer", "Winter", "Spring"}, 1, "Winter is usually the coldest season."),
            new Question(2015, "Which tool is used to cut paper?", new[] {"Scissors", "Ruler", "Glue"}, 0, "Scissors are used for cutting."),
            new Question(2016, "What is 3 + 4?", new[] {"6", "7", "8"}, 1, "3 plus 4 equals 7."),
            new Question(2017, "Which is used to tell time?", new[] {"Clock", "Book", "Spoon"}, 0, "A clock tells time."),
            new Question(2018, "Which animal can fly?", new[] {"Fish", "Bird", "Turtle"}, 1, "Birds can fly."),
            new Question(2019, "What do plants need to grow?", new[] {"Sunlight & water", "Sand only", "Plastic"}, 0, "Plants need sunlight, water, and nutrients."),
            new Question(2020, "Which is a primary color?", new[] {"Purple", "Green", "Red"}, 2, "Red is a primary color (along with blue and yellow)."),
        };


        _byDiff[LadderDifficulty.Normal] = new List<Question>
        {
            new Question(2001, "Normal Q1?", new []{"A","B","C"}, 0, "Because A."),
            // ...
        };

        _byDiff[LadderDifficulty.Hard] = new List<Question>
        {
            new Question(3001, "Hard Q1?", new []{"A","B","C"}, 1, "Because B."),
            // ...
        };

        _byDiff[LadderDifficulty.Advanced] = new List<Question>
        {
            new Question(4001, "Advanced Q1?", new []{"A","B","C"}, 2, "Because C."),
            // ...
        };

        _byDiff[LadderDifficulty.Expert] = new List<Question>
        {
            new Question(5001, "Expert Q1?", new []{"A","B","C"}, 0, "Because A."),
            // ...
        };
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
