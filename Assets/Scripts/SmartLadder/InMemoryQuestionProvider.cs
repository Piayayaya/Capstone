using System.Collections.Generic;
using UnityEngine;

public class InMemoryQuestionProvider : IQuestionProvider
{
    private readonly List<Question> _all = new List<Question>();

    public void Initialize()
    {
        // --- 20 EASY QUESTIONS (3 choices + explanation) ---
        _all.Add(new Question
        {
            Id = 1,
            Difficulty = LadderDifficulty.Easy,
            Text = "What planet do we live on?",
            Choices = new[] { "Mars", "Earth", "Venus" },
            CorrectIndex = 1,
            Explanation = "We live on Earth, the third planet from the Sun."
        });

        _all.Add(new Question
        {
            Id = 2,
            Difficulty = LadderDifficulty.Easy,
            Text = "2 + 3 = ?",
            Choices = new[] { "4", "5", "6" },
            CorrectIndex = 1,
            Explanation = "2 plus 3 equals 5."
        });

        _all.Add(new Question
        {
            Id = 3,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which animal is a mammal?",
            Choices = new[] { "Shark", "Dolphin", "Salmon" },
            CorrectIndex = 1,
            Explanation = "Dolphins are warm-blooded, breathe air, and feed milk to their young."
        });

        _all.Add(new Question
        {
            Id = 4,
            Difficulty = LadderDifficulty.Easy,
            Text = "What shape has 3 sides?",
            Choices = new[] { "Square", "Triangle", "Circle" },
            CorrectIndex = 1,
            Explanation = "A triangle has exactly three sides."
        });

        _all.Add(new Question
        {
            Id = 5,
            Difficulty = LadderDifficulty.Easy,
            Text = "Water freezes at what temperature (°C)?",
            Choices = new[] { "0", "50", "100" },
            CorrectIndex = 0,
            Explanation = "Pure water freezes at 0°C."
        });

        _all.Add(new Question
        {
            Id = 6,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which is the largest land animal?",
            Choices = new[] { "Elephant", "Tiger", "Horse" },
            CorrectIndex = 0,
            Explanation = "The African elephant is the largest land animal."
        });

        _all.Add(new Question
        {
            Id = 7,
            Difficulty = LadderDifficulty.Easy,
            Text = "Plants make their own food using…",
            Choices = new[] { "Photosynthesis", "Breathing", "Digestion" },
            CorrectIndex = 0,
            Explanation = "Photosynthesis uses sunlight, water, and carbon dioxide to make food."
        });

        _all.Add(new Question
        {
            Id = 8,
            Difficulty = LadderDifficulty.Easy,
            Text = "How many continents are there on Earth?",
            Choices = new[] { "5", "6", "7" },
            CorrectIndex = 2,
            Explanation = "Most models teach 7 continents."
        });

        _all.Add(new Question
        {
            Id = 9,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which of these is a primary color of light/paint sets?",
            Choices = new[] { "Green", "Red", "Purple" },
            CorrectIndex = 1,
            Explanation = "Red is a primary color (with blue and yellow in many school sets)."
        });

        _all.Add(new Question
        {
            Id = 10,
            Difficulty = LadderDifficulty.Easy,
            Text = "What day comes after Friday?",
            Choices = new[] { "Saturday", "Sunday", "Monday" },
            CorrectIndex = 0,
            Explanation = "Friday is followed by Saturday."
        });

        _all.Add(new Question
        {
            Id = 11,
            Difficulty = LadderDifficulty.Easy,
            Text = "10 − 4 = ?",
            Choices = new[] { "5", "6", "7" },
            CorrectIndex = 1,
            Explanation = "10 minus 4 equals 6."
        });

        _all.Add(new Question
        {
            Id = 12,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which gas makes up most of the air we breathe?",
            Choices = new[] { "Oxygen", "Nitrogen", "Carbon dioxide" },
            CorrectIndex = 1,
            Explanation = "Air is ~78% nitrogen. We breathe oxygen, but nitrogen is most abundant."
        });

        _all.Add(new Question
        {
            Id = 13,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which shape has 4 equal sides?",
            Choices = new[] { "Rectangle", "Square", "Trapezoid" },
            CorrectIndex = 1,
            Explanation = "A square has four equal sides and four right angles."
        });

        _all.Add(new Question
        {
            Id = 14,
            Difficulty = LadderDifficulty.Easy,
            Text = "What is the closest star to Earth?",
            Choices = new[] { "Sirius", "The Sun", "Polaris" },
            CorrectIndex = 1,
            Explanation = "The Sun is our nearest star."
        });

        _all.Add(new Question
        {
            Id = 15,
            Difficulty = LadderDifficulty.Easy,
            Text = "A baby frog is called a…",
            Choices = new[] { "Chick", "Tadpole", "Calf" },
            CorrectIndex = 1,
            Explanation = "Frogs hatch into tadpoles before becoming adults."
        });

        _all.Add(new Question
        {
            Id = 16,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which instrument has keys and is played with both hands?",
            Choices = new[] { "Guitar", "Piano", "Drum" },
            CorrectIndex = 1,
            Explanation = "A piano has keys played with both hands."
        });

        _all.Add(new Question
        {
            Id = 17,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which word is a noun?",
            Choices = new[] { "Run", "Happiness", "Quickly" },
            CorrectIndex = 1,
            Explanation = "“Happiness” is a thing/idea, so it’s a noun."
        });

        _all.Add(new Question
        {
            Id = 18,
            Difficulty = LadderDifficulty.Easy,
            Text = "What do bees make?",
            Choices = new[] { "Honey", "Milk", "Silk" },
            CorrectIndex = 0,
            Explanation = "Bees collect nectar and make honey."
        });

        _all.Add(new Question
        {
            Id = 19,
            Difficulty = LadderDifficulty.Easy,
            Text = "How many minutes are in one hour?",
            Choices = new[] { "30", "45", "60" },
            CorrectIndex = 2,
            Explanation = "There are 60 minutes in an hour."
        });

        _all.Add(new Question
        {
            Id = 20,
            Difficulty = LadderDifficulty.Easy,
            Text = "Which of these is an island country in Southeast Asia?",
            Choices = new[] { "Philippines", "Nepal", "Mongolia" },
            CorrectIndex = 0,
            Explanation = "The Philippines is an archipelago in Southeast Asia."
        });
    }

    public Question GetNext(LadderDifficulty difficulty, HashSet<int> alreadyAsked)
    {
        // Gather pool by difficulty
        // (No LINQ to keep it beginner-friendly)
        var pool = new List<Question>();
        for (int i = 0; i < _all.Count; i++)
        {
            if (_all[i].Difficulty == difficulty) pool.Add(_all[i]);
        }
        if (pool.Count == 0) return null;

        // Prefer unasked this run
        var fresh = new List<Question>();
        for (int i = 0; i < pool.Count; i++)
        {
            if (!alreadyAsked.Contains(pool[i].Id)) fresh.Add(pool[i]);
        }
        var list = (fresh.Count > 0) ? fresh : pool;

        int idx = Random.Range(0, list.Count);
        return list[idx];
    }
}
