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
            new Question(3001, "What is 7 × 6?", new[] { "42", "36", "48" }, 0, "7 groups of 6 is 42."),
            new Question(3002, "Which fraction is equal to 1/2?", new[] { "3/6", "2/3", "1/3" }, 0, "3 out of 6 is half."),
            new Question(3003, "What is the next number? 9, 12, 15, __", new[] { "18", "17", "16" }, 0, "It adds 3 each time."),
            new Question(3004, "Which is a noun?", new[] { "Mountain", "Quickly", "Blue" }, 0, "A noun names a person, place, or thing."),
            new Question(3005, "A rectangle is 8 cm long and 3 cm wide. Its perimeter is…", new[] { "22 cm", "24 cm", "16 cm" }, 0, "Perimeter = 2×(8+3)=22."),
            new Question(3006, "What time is a quarter past 4?", new[] { "4:15", "4:25", "4:45" }, 0, "A quarter past is :15."),
            new Question(3007, "Which is the synonym of 'happy'?", new[] { "Joyful", "Angry", "Sleepy" }, 0, "Joyful means happy."),
            new Question(3008, "Which planet is known as the Red Planet?", new[] { "Mars", "Venus", "Jupiter" }, 0, "Mars looks red from iron dust."),
            new Question(3009, "Solve: 84 ÷ 7", new[] { "12", "14", "10" }, 0, "7×12=84."),
            new Question(3010, "Which is larger?", new[] { "3/4", "2/3", "1/2" }, 0, "0.75 is bigger than 0.66 and 0.5."),
            new Question(3011, "Which sentence is punctuated correctly?", new[] { "Where is my book?", "Where is my book.", "Where is my book" }, 0, "Questions end with a question mark."),
            new Question(3012, "How many edges does a cube have?", new[] { "12", "8", "6" }, 0, "A cube has 12 edges."),
            new Question(3013, "What is the value of the digit 6 in 6,452?", new[] { "Six thousands", "Six hundreds", "Six tens" }, 0, "It’s in the thousands place."),
            new Question(3014, "Which is an example of a vertebrate?", new[] { "Frog", "Earthworm", "Spider" }, 0, "Frogs have backbones."),
            new Question(3015, "What is the synonym of 'small'?", new[] { "Tiny", "Huge", "Loud" }, 0, "Tiny means small."),
            new Question(3016, "A triangle has angles 30°, 60°, and __ to make 180°.", new[] { "90°", "120°", "80°" }, 0, "30+60+90=180."),
            new Question(3017, "Which fraction is the same as 4/8?", new[] { "1/2", "2/3", "3/4" }, 0, "Divide top and bottom by 4."),
            new Question(3018, "What is 300 + 450?", new[] { "750", "730", "740" }, 0, "3 hundreds + 4 hundreds + 50 = 750."),
            new Question(3019, "Which is a complete sentence?", new[] { "The dog ran.", "Because the dog.", "Running fast." }, 0, "It has subject and verb."),
            new Question(3020, "Which direction is opposite of east?", new[] { "West", "North", "South" }, 0, "East ↔ West."),
            new Question(3021, "Which unit is best to measure a pencil?", new[] { "Centimeters", "Kilometers", "Liters" }, 0, "Centimeters measure small lengths."),
            new Question(3022, "What is the remainder of 29 ÷ 5?", new[] { "4", "3", "2" }, 0, "5×5=25; 29−25=4."),
            new Question(3023, "Which word is an adjective?", new[] { "Spiky", "Carefully", "Jump" }, 0, "Adjectives describe nouns."),
            new Question(3024, "Which number is prime?", new[] { "13", "15", "21" }, 0, "13 has factors 1 and 13."),
            new Question(3025, "A book costs $9 and a pen costs $3. Total?", new[] { "$12", "$13", "$11" }, 0, "9+3=12."),
            new Question(3026, "Which is a renewable energy source?", new[] { "Wind", "Coal", "Oil" }, 0, "Wind is renewable."),
            new Question(3027, "Round 687 to the nearest ten.", new[] { "690", "680", "700" }, 0, "7 rounds up."),
            new Question(3028, "What’s the next shape: ▲, ■, ▲, ■, __", new[] { "▲", "●", "■" }, 0, "Triangle, square repeating."),
            new Question(3029, "Which ocean is the largest?", new[] { "Pacific", "Atlantic", "Indian" }, 0, "The Pacific is largest."),
            new Question(3030, "Which word is the antonym of 'noisy'?", new[] { "Quiet", "Bright", "Swift" }, 0, "Quiet is the opposite of noisy."),
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
