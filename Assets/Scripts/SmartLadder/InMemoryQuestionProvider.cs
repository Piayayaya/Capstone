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
            new Question(2003, "Which day comes after Monday?", new[] {"Sunday", "Tuesday", "Friday"}, 1, "The order is Monday -> Tuesday."),
            new Question(2004, "How many sides does a triangle have?", new[] {"3", "4", "5"}, 0, "A triangle has 3 sides."),
            new Question(2005, "What color do you get by mixing red and blue?", new[] {"Purple", "Green", "Orange"}, 0, "Red + blue = purple."),
            new Question(2006, "Which number is the smallest?", new[] {"7", "2", "9"}, 1, "2 is smaller than 7 and 9."),
            new Question(2007, "What is the first letter of the word 'Apple'?", new[] {"A", "P", "L"}, 0, "Apple starts with A."),
            new Question(2008, "Which one is a fruit?", new[] {"Carrot", "Banana", "Broccoli"}, 1, "Banana is a fruit; the others are vegetables."),
            new Question(2009, "How many legs does a spider have?", new[] {"6", "8", "10"}, 1, "Spiders have 8 legs."),
            new Question(2010, "What is 10 - 4?", new[] {"5", "6", "7"}, 1, "10 minus 4 equals 6."),
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
            new Question(3101, "What is 6 + 7?", new[] { "13", "12", "14" }, 0, "6 plus 7 equals 13."),
            new Question(3102, "What is 9 - 4?", new[] { "5", "6", "4" }, 0, "9 minus 4 equals 5."),
            new Question(3103, "What is 3 × 4?", new[] { "12", "9", "8" }, 0, "3 groups of 4 is 12."),
            new Question(3104, "What is the number just before 100?", new[] { "99", "90", "98" }, 0, "The number before 100 is 99."),
            new Question(3105, "Which planet do we live on?", new[] { "Earth", "Mars", "Jupiter" }, 0, "Humans live on planet Earth."),
            new Question(3106, "Which of these animals is a mammal?", new[] { "Dolphin", "Frog", "Eagle" }, 0, "Dolphins are mammals that breathe air."),
            new Question(3107, "30 minutes after 3:00 is what time?", new[] { "3:30", "3:15", "4:00" }, 0, "Adding 30 minutes to 3:00 gives 3:30."),
            new Question(3108, "If a pizza is cut into 4 equal slices and you eat 1 slice, what fraction did you eat?", new[] { "1/4", "1/2", "1/3" }, 0, "You ate 1 of 4 equal pieces, so 1/4."),
            new Question(3109, "What is the plural of child?", new[] { "Children", "Childs", "Childes" }, 0, "The correct plural is children."),
            new Question(3110, "Which gas do humans need most to breathe?", new[] { "Oxygen", "Carbon dioxide", "Helium" }, 0, "We breathe in oxygen to stay alive."),
            new Question(3111, "Which tool is used mainly to cut paper?", new[] { "Scissors", "Spoon", "Fork" }, 0, "Scissors are used for cutting paper."),
            new Question(3112, "What is 1 hour equal to?", new[] { "60 minutes", "30 minutes", "100 minutes" }, 0, "1 hour has 60 minutes."),
            new Question(3113, "Which of these animals can fly?", new[] { "Eagle", "Dog", "Cat" }, 0, "Eagles are birds that can fly."),
            new Question(3114, "Which number comes next in the pattern 5, 10, 15, ...?", new[] { "20", "18", "25" }, 0, "You add 5 each time, so 15 + 5 = 20."),
            new Question(3115, "Which of these is a source of light?", new[] { "Sun", "Rock", "Pillow" }, 0, "The Sun gives us light."),
            new Question(3116, "What is 12 + 9?", new[] { "21", "20", "19" }, 0, "12 plus 9 equals 21."),
            new Question(3117, "What is 18 - 7?", new[] { "11", "9", "10" }, 0, "18 minus 7 equals 11."),
            new Question(3118, "Which fraction shows half of a pizza?", new[] { "1/2", "1/3", "2/3" }, 0, "One half is written as 1/2."),
            new Question(3119, "Which of these is a solid?", new[] { "Ice cube", "Water", "Steam" }, 0, "Ice is water in solid form."),
            new Question(3120, "Which month comes after August?", new[] { "September", "July", "October" }, 0, "September comes after August."),
            new Question(3121, "What is 4 × 5?", new[] { "20", "16", "25" }, 0, "4 times 5 equals 20."),
            new Question(3122, "What is 35 ÷ 5?", new[] { "7", "6", "5" }, 0, "35 divided by 5 equals 7."),
            new Question(3123, "Which of these is a verb?", new[] { "Run", "Yellow", "Happy" }, 0, "Run is an action word, so it is a verb."),
            new Question(3124, "Which of these is a proper noun?", new[] { "Philippines", "country", "city" }, 0, "Philippines is the name of a country, so it is a proper noun."),
            new Question(3125, "Which direction is opposite of north?", new[] { "South", "East", "West" }, 0, "South is opposite of north."),
            new Question(3126, "What is 22 + 8?", new[] { "30", "28", "32" }, 0, "22 plus 8 equals 30."),
            new Question(3127, "What is 40 - 19?", new[] { "21", "20", "22" }, 0, "40 minus 19 equals 21."),
            new Question(3128, "Which part of the plant holds it in the soil?", new[] { "Roots", "Leaves", "Flower" }, 0, "Roots hold the plant in the ground."),
            new Question(3129, "Which of these is a liquid at room temperature?", new[] { "Water", "Ice", "Metal spoon" }, 0, "Water is a liquid at room temperature."),
            new Question(3130, "Which sense organ do you use to hear?", new[] { "Ears", "Eyes", "Nose" }, 0, "We use our ears to hear sounds."),
        };


        _byDiff[LadderDifficulty.Hard] = new List<Question>
        {
            new Question(3201, "What is 7 × 6?", new[] { "42", "36", "48" }, 0, "7 groups of 6 is 42."),
            new Question(3202, "What is 48 ÷ 6?", new[] { "8", "6", "7" }, 0, "48 divided by 6 equals 8."),
            new Question(3203, "A square has sides of length 4 cm. What is its perimeter?", new[] { "16 cm", "12 cm", "8 cm" }, 0, "A square has 4 equal sides, so 4 + 4 + 4 + 4 = 16 cm."),
            new Question(3204, "What is 125 + 37?", new[] { "162", "152", "172" }, 0, "125 plus 37 equals 162."),
            new Question(3205, "What is 100 - 37?", new[] { "63", "73", "67" }, 0, "100 minus 37 equals 63."),
            new Question(3206, "What is 9 × 5?", new[] { "45", "40", "35" }, 0, "9 times 5 equals 45."),
            new Question(3207, "What is 24 ÷ 3?", new[] { "8", "6", "9" }, 0, "24 divided by 3 equals 8."),
            new Question(3208, "What is the simplest form of the fraction 2/4?", new[] { "1/2", "2/2", "1/4" }, 0, "2/4 can be simplified to 1/2."),
            new Question(3209, "Which fraction is the largest?", new[] { "1/2", "1/4", "1/8" }, 0, "1/2 is bigger than 1/4 and 1/8."),
            new Question(3210, "Which of these numbers is a prime number?", new[] { "11", "9", "12" }, 0, "11 has only two factors, 1 and 11."),
            new Question(3211, "Which organ pumps blood around your body?", new[] { "Heart", "Lungs", "Stomach" }, 0, "The heart pumps blood through your body."),
            new Question(3212, "The Sun is a:", new[] { "Star", "Planet", "Moon" }, 0, "The Sun is a star at the center of our solar system."),
            new Question(3213, "Water freezes at what temperature (in °C)?", new[] { "0 °C", "10 °C", "100 °C" }, 0, "Water freezes at 0 °C."),
            new Question(3214, "In a 24-hour clock, what time is 18:00?", new[] { "6:00 PM", "8:00 PM", "10:00 PM" }, 0, "18:00 is 6:00 PM in 12-hour time."),
            new Question(3215, "You have 12 candies and share them equally with 3 friends (4 people total). How many candies does each get?", new[] { "3", "4", "2" }, 0, "12 divided by 4 equals 3 each."),
            new Question(3216, "In the number 345, what is the place value of the digit 4?", new[] { "Tens", "Hundreds", "Ones" }, 0, "3 is hundreds, 4 is tens, 5 is ones."),
            new Question(3217, "What is 15 × 0?", new[] { "0", "15", "1" }, 0, "Any number times 0 equals 0."),
            new Question(3218, "What is 25% of 100?", new[] { "25", "50", "75" }, 0, "25% means 25 out of 100."),
            new Question(3219, "Which word is a noun?", new[] { "Cat", "Run", "Sing" }, 0, "A cat is a thing, so it is a noun."),
            new Question(3220, "Which word is an adjective?", new[] { "Happy", "Quickly", "Sing" }, 0, "Happy describes how someone feels, so it is an adjective."),
            new Question(3221, "What is the opposite of hot?", new[] { "Cold", "Warm", "Dry" }, 0, "Cold is the opposite of hot."),
            new Question(3222, "Which of these is a renewable energy source?", new[] { "Solar power", "Coal", "Oil" }, 0, "Solar power comes from the Sun and is renewable."),
            new Question(3223, "Which of these animals lays eggs?", new[] { "Chicken", "Cat", "Horse" }, 0, "Chickens are birds and lay eggs."),
            new Question(3224, "Which sense organ do we use to see?", new[] { "Eyes", "Ears", "Tongue" }, 0, "We use our eyes to see."),
            new Question(3225, "How many sides does a hexagon have?", new[] { "6", "5", "8" }, 0, "A hexagon has 6 sides."),
            new Question(3226, "What is 63 ÷ 7?", new[] { "9", "7", "8" }, 0, "63 divided by 7 equals 9."),
            new Question(3227, "What is 18 × 4?", new[] { "72", "64", "68" }, 0, "18 times 4 equals 72."),
            new Question(3228, "What is the value of 300 - 125?", new[] { "175", "185", "195" }, 0, "300 minus 125 equals 175."),
            new Question(3229, "Which fraction is equivalent to 2/3?", new[] { "4/6", "3/4", "2/5" }, 0, "If you multiply top and bottom of 2/3 by 2, you get 4/6."),
            new Question(3230, "Which angle is greater than a right angle?", new[] { "Obtuse angle", "Acute angle", "Straight angle" }, 0, "An obtuse angle is more than 90 degrees."),
            new Question(3231, "What is the perimeter of a triangle with sides 3 cm, 5 cm, and 7 cm?", new[] { "15 cm", "14 cm", "13 cm" }, 0, "Add the sides: 3 + 5 + 7 = 15 cm."),
            new Question(3232, "Which blood vessels carry blood back to the heart?", new[] { "Veins", "Arteries", "Capillaries" }, 0, "Veins carry blood back to the heart."),
            new Question(3233, "Which gas in the air do we mostly breathe in?", new[] { "Nitrogen", "Oxygen", "Carbon dioxide" }, 0, "Most of the air is nitrogen."),
            new Question(3234, "Which part of a plant makes seeds?", new[] { "Flower", "Root", "Stem" }, 0, "Flowers usually make seeds."),
            new Question(3235, "Which is the correct past tense of 'go'?", new[] { "Went", "Goed", "Gone" }, 0, "The correct past tense is 'went'."),
            new Question(3236, "Which of these is a complete sentence?", new[] { "The boy ran fast.", "Because the boy ran.", "When the boy ran." }, 0, "A complete sentence has a full idea: 'The boy ran fast.'"),
            new Question(3237, "Which punctuation mark shows strong feeling?", new[] { "Exclamation mark", "Comma", "Colon" }, 0, "We use an exclamation mark for strong feelings."),
            new Question(3238, "Which layer of Earth is the hottest?", new[] { "Core", "Crust", "Mantle" }, 0, "The core is the hottest part of Earth."),
            new Question(3239, "What is 3/10 written as a decimal?", new[] { "0.3", "0.03", "0.13" }, 0, "3 tenths is 0.3."),
            new Question(3240, "Which of these is a mixture?", new[] { "Salt water", "Gold bar", "Pure water" }, 0, "Salt water is salt mixed with water."),
            new Question(3241, "Which property of matter describes how much space it takes up?", new[] { "Volume", "Mass", "Weight" }, 0, "Volume is the space an object takes up."),
            new Question(3242, "Which simple machine is found in a screw?", new[] { "Inclined plane", "Lever", "Pulley" }, 0, "A screw is like an inclined plane wrapped around a rod."),
            new Question(3243, "Which of these is a homophone pair?", new[] { "Two and too", "Cat and dog", "Big and small" }, 0, "Two and too sound the same but have different meanings."),
            new Question(3244, "Which is the correct comparison? 3/4 is...", new[] { "Greater than 1/2", "Less than 1/2", "Equal to 1/2" }, 0, "3/4 is more than 1/2."),
            new Question(3245, "What is the value of the digit 6 in 6,241?", new[] { "6000", "600", "60" }, 0, "The 6 is in the thousands place, so it means 6000."),
        };

        _byDiff[LadderDifficulty.Advanced] = new List<Question>
        {
            new Question(3301, "What is 14 × 3?", new[] { "42", "36", "44" }, 0, "14 times 3 equals 42."),
            new Question(3302, "What is 96 ÷ 8?", new[] { "12", "10", "8" }, 0, "96 divided by 8 equals 12."),
            new Question(3303, "Solve for x: x + 5 = 12", new[] { "7", "6", "8" }, 0, "Subtract 5 from both sides: x = 12 - 5 = 7."),
            new Question(3304, "A rectangle is 8 cm long and 3 cm wide. What is its area?", new[] { "24 cm²", "11 cm²", "16 cm²" }, 0, "Area is length × width, so 8 × 3 = 24 cm²."),
            new Question(3305, "What is 250 - 175?", new[] { "75", "85", "65" }, 0, "250 minus 175 equals 75."),
            new Question(3306, "What is 7² (7 squared)?", new[] { "49", "14", "21" }, 0, "7 squared means 7 × 7, which is 49."),
            new Question(3307, "Which fraction is equal to 0.5?", new[] { "1/2", "1/3", "2/3" }, 0, "0.5 is the same as one half, 1/2."),
            new Question(3308, "A book costs 85 coins and you have 100 coins. How many coins will you have left?", new[] { "15", "10", "5" }, 0, "100 - 85 = 15 coins left."),
            new Question(3309, "Which unit would you use to measure the length of a pencil?", new[] { "Centimeters", "Kilometers", "Liters" }, 0, "Centimeters are good for short lengths."),
            new Question(3310, "Which of these is NOT a prime number?", new[] { "15", "13", "17" }, 0, "15 has more than two factors."),
            new Question(3311, "Which part of a plant makes food using sunlight?", new[] { "Leaves", "Roots", "Stem" }, 0, "Leaves use sunlight to make food."),
            new Question(3312, "What force pulls objects towards the Earth?", new[] { "Gravity", "Friction", "Magnetism" }, 0, "Gravity pulls objects toward Earth."),
            new Question(3313, "Which planet is known as the Red Planet?", new[] { "Mars", "Venus", "Mercury" }, 0, "Mars is called the Red Planet."),
            new Question(3314, "Which of these is a vertebrate?", new[] { "Fish", "Earthworm", "Snail" }, 0, "Fish have backbones."),
            new Question(3315, "Which is the correct spelling?", new[] { "Because", "Becouse", "Becase" }, 0, "The correct spelling is 'because'."),
            new Question(3316, "Which sentence uses the correct past tense?", new[] { "I walked to school.", "I walk to school yesterday.", "I am walk to school." }, 0, "'I walked to school.' is correct."),
            new Question(3317, "Which punctuation mark ends a question?", new[] { "Question mark", "Comma", "Colon" }, 0, "Questions end with a question mark."),
            new Question(3318, "Which word in this sentence is an adverb: 'She ran quickly.'?", new[] { "Quickly", "She", "Ran" }, 0, "Quickly tells how she ran."),
            new Question(3319, "What is the main function of the lungs?", new[] { "To help us breathe", "To pump blood", "To digest food" }, 0, "Lungs help us take in oxygen."),
            new Question(3320, "Which part of the plant is usually underground?", new[] { "Roots", "Leaves", "Flowers" }, 0, "Roots are usually underground."),
            new Question(3321, "How many degrees are in a right angle?", new[] { "90°", "45°", "180°" }, 0, "A right angle is 90 degrees."),
            new Question(3322, "What is the perimeter of a rectangle with sides 5 cm and 7 cm?", new[] { "24 cm", "20 cm", "35 cm" }, 0, "5 + 7 + 5 + 7 = 24 cm."),
            new Question(3323, "What is the value of 3/5 of 20?", new[] { "12", "10", "8" }, 0, "3/5 of 20 is 12."),
            new Question(3324, "Which number is a multiple of both 3 and 4?", new[] { "12", "9", "16" }, 0, "12 is divisible by 3 and 4."),
            new Question(3325, "If a train leaves at 2:15 and arrives at 3:00, how long is the trip?", new[] { "45 minutes", "30 minutes", "60 minutes" }, 0, "From 2:15 to 3:00 is 45 minutes."),
            new Question(3326, "Which of these is a simile?", new[] { "He is as fast as lightning.", "He runs fast.", "He is lightning." }, 0, "A simile uses 'as' or 'like'."),
            new Question(3327, "Which layer of Earth do we live on?", new[] { "Crust", "Core", "Mantle" }, 0, "We live on Earth's crust."),
            new Question(3328, "Which simple machine is a seesaw?", new[] { "Lever", "Pulley", "Wheel and axle" }, 0, "A seesaw is a lever."),
            new Question(3329, "Which change of state happens when ice turns into water?", new[] { "Melting", "Freezing", "Evaporation" }, 0, "Ice turning to water is melting."),
            new Question(3330, "Which of these is a conductor of electricity?", new[] { "Metal spoon", "Rubber eraser", "Wooden stick" }, 0, "Metals conduct electricity."),
            new Question(3331, "What is the median of the numbers 2, 5, 7, 8, 10?", new[] { "7", "5", "8" }, 0, "The middle number is 7."),
            new Question(3332, "What is the mode of the numbers 4, 4, 6, 7, 7, 7?", new[] { "7", "4", "6" }, 0, "7 appears most often."),
            new Question(3333, "Which ocean is the largest on Earth?", new[] { "Pacific Ocean", "Atlantic Ocean", "Indian Ocean" }, 0, "The Pacific Ocean is the largest."),
            new Question(3334, "Which scientist is famous for the three laws of motion?", new[] { "Isaac Newton", "Albert Einstein", "Marie Curie" }, 0, "Isaac Newton made the three laws of motion."),
            new Question(3335, "Which process do green plants use to make food?", new[] { "Photosynthesis", "Respiration", "Evaporation" }, 0, "Plants use photosynthesis to make food."),
            new Question(3336, "What is 3³ (3 cubed)?", new[] { "27", "9", "6" }, 0, "3 × 3 × 3 = 27."),
            new Question(3337, "What is 45% of 100?", new[] { "45", "40", "55" }, 0, "45% of 100 is 45."),
            new Question(3338, "Which fraction is greater?", new[] { "5/8", "1/2", "3/8" }, 0, "5/8 is larger than 1/2 and 3/8."),
            new Question(3339, "A triangle has angles 60°, 60°, and 60°. What kind of triangle is it?", new[] { "Equilateral", "Right", "Scalene" }, 0, "All angles equal means equilateral."),
            new Question(3340, "What is the area of a square with side 9 cm?", new[] { "81 cm²", "18 cm²", "36 cm²" }, 0, "Area = side × side = 9 × 9 = 81."),
            new Question(3341, "Which group of numbers are all even?", new[] { "4, 8, 12", "3, 8, 11", "5, 7, 9" }, 0, "4, 8, and 12 are all even."),
            new Question(3342, "Which of these is a metaphor?", new[] { "Time is a thief.", "Time moves like a snail.", "Time goes slowly." }, 0, "Metaphors say one thing is another."),
            new Question(3343, "Which sentence is in future tense?", new[] { "I will study later.", "I studied yesterday.", "I am studying now." }, 0, "'I will study later.' is future tense."),
            new Question(3344, "What is the main idea of a paragraph?", new[] { "The most important point", "A random detail", "The longest sentence" }, 0, "The main idea is the most important point."),
            new Question(3345, "Which group contains only mammals?", new[] { "Dog, whale, bat", "Dog, snake, bat", "Frog, whale, bat" }, 0, "Dogs, whales, and bats are mammals."),
            new Question(3346, "Which change is reversible?", new[] { "Freezing water", "Burning paper", "Rusting iron" }, 0, "Frozen water can melt back to liquid."),
            new Question(3347, "Which object has the most friction on the floor?", new[] { "Rubber shoe", "Ice cube", "Oiled metal" }, 0, "Rubber on the floor has high friction."),
            new Question(3348, "Which part of the Earth has frozen water?", new[] { "Poles", "Equator", "Mantle" }, 0, "The North and South Poles have lots of ice."),
            new Question(3349, "Which of these is a solution?", new[] { "Sugar dissolved in water", "Sand in water", "Stones in water" }, 0, "Sugar dissolves to make a solution."),
            new Question(3350, "What type of weather is measured by a rain gauge?", new[] { "Rainfall", "Wind", "Temperature" }, 0, "A rain gauge measures how much rain falls."),
            new Question(3351, "Which is an example of physical change?", new[] { "Cutting paper", "Burning wood", "Baking bread" }, 0, "Cutting paper only changes its shape."),
            new Question(3352, "Which scientist is known for discovering that germs can cause disease?", new[] { "Louis Pasteur", "Isaac Newton", "Albert Einstein" }, 0, "Louis Pasteur studied germs and disease."),
            new Question(3353, "Which is the smallest unit of life?", new[] { "Cell", "Organ", "System" }, 0, "The cell is the basic unit of life."),
            new Question(3354, "Which of these is a producer in a food chain?", new[] { "Grass", "Lion", "Wolf" }, 0, "Grass makes its own food and is a producer."),
            new Question(3355, "Which organ helps filter waste from the blood?", new[] { "Kidneys", "Heart", "Lungs" }, 0, "The kidneys filter waste from the blood."),
            new Question(3356, "Which of these is an example of renewable energy?", new[] { "Wind", "Coal", "Gasoline" }, 0, "Wind can be used again and again."),
            new Question(3357, "Which statement about mass is true?", new[] { "Mass stays the same even on the Moon.", "Mass changes when you move to the Moon.", "Mass disappears in space." }, 0, "Your mass does not change with location."),
            new Question(3358, "What is the probability of getting heads on a fair coin?", new[] { "1/2", "1/3", "1/4" }, 0, "A fair coin has 2 sides, so 1 out of 2."),
            new Question(3359, "If 3x = 27, what is x?", new[] { "9", "6", "8" }, 0, "27 ÷ 3 = 9."),
            new Question(3360, "If the perimeter of a square is 32 cm, what is the length of one side?", new[] { "8 cm", "6 cm", "4 cm" }, 0, "32 ÷ 4 sides = 8 cm each."),
        };

        _byDiff[LadderDifficulty.Expert] = new List<Question>
        {
            new Question(3401, "Solve for x: 3x = 21", new[] { "7", "6", "8" }, 0, "Divide both sides by 3 to get x = 7."),
            new Question(3402, "Solve for x: x/4 = 5", new[] { "20", "9", "16" }, 0, "Multiply both sides by 4: x = 20."),
            new Question(3403, "What is 18 × 7?", new[] { "126", "124", "136" }, 0, "18 times 7 equals 126."),
            new Question(3404, "What is 144 ÷ 12?", new[] { "12", "10", "14" }, 0, "144 divided by 12 equals 12."),
            new Question(3405, "A box is 5 cm long, 4 cm wide, and 3 cm high. What is its volume?", new[] { "60 cm³", "20 cm³", "12 cm³" }, 0, "Volume = 5 × 4 × 3 = 60 cm³."),
            new Question(3406, "What is the value of 2³?", new[] { "8", "6", "4" }, 0, "2³ means 2 × 2 × 2 = 8."),
            new Question(3407, "Which of these is equal to 3/4?", new[] { "0.75", "0.3", "0.25" }, 0, "3/4 as a decimal is 0.75."),
            new Question(3408, "You scored 18 out of 20 on a test. What is your score as a percentage?", new[] { "90%", "80%", "75%" }, 0, "18/20 = 0.9 or 90%."),
            new Question(3409, "What is the mean of 6, 8, 10, and 12?", new[] { "9", "8", "10" }, 0, "The sum is 36, divided by 4 equals 9."),
            new Question(3410, "A triangle has sides of 3 cm, 4 cm, and 5 cm. What kind of triangle is it?", new[] { "Right triangle", "Equilateral triangle", "Isosceles triangle" }, 0, "3, 4, and 5 make a right triangle."),
            new Question(3411, "Which of these is a compound sentence?", new[] { "I was tired, so I went to bed.", "I was tired.", "I went to bed." }, 0, "A compound sentence joins two ideas with 'so'."),
            new Question(3412, "Which sentence uses the correct subject-verb agreement?", new[] { "The dogs run fast.", "The dogs runs fast.", "The dog run fast." }, 0, "For plural 'dogs' we use 'run'."),
            new Question(3413, "In the sentence 'The quick brown fox jumps over the lazy dog', which word is an adjective?", new[] { "Quick", "Jumps", "Dog" }, 0, "'Quick' describes the fox."),
            new Question(3414, "Which of these is an example of personification?", new[] { "The wind whispered through the trees.", "The wind was strong.", "The trees were tall." }, 0, "Personification gives human actions like 'whispered' to things."),
            new Question(3415, "Which of these is a synonym of brave?", new[] { "Courageous", "Afraid", "Shy" }, 0, "Courageous means brave."),
            new Question(3416, "What is the main function of the heart?", new[] { "To pump blood", "To control thinking", "To help breathing" }, 0, "The heart pumps blood around the body."),
            new Question(3417, "Which blood vessels carry blood away from the heart?", new[] { "Arteries", "Veins", "Capillaries" }, 0, "Arteries carry blood away from the heart."),
            new Question(3418, "Where in the body does most digestion happen?", new[] { "Small intestine", "Lungs", "Brain" }, 0, "Most digestion happens in the small intestine."),
            new Question(3419, "Which part of the eye controls how much light enters?", new[] { "Pupil", "Eyelash", "Eyebrow" }, 0, "The pupil controls how much light enters."),
            new Question(3420, "Which gas do plants take in during photosynthesis?", new[] { "Carbon dioxide", "Oxygen", "Nitrogen" }, 0, "Plants take in carbon dioxide."),
            new Question(3421, "Which gas do plants release during photosynthesis?", new[] { "Oxygen", "Carbon dioxide", "Helium" }, 0, "Plants release oxygen."),
            new Question(3422, "Which simple machine is used in a flagpole to raise the flag?", new[] { "Pulley", "Lever", "Inclined plane" }, 0, "A flagpole uses a pulley."),
            new Question(3423, "What type of energy does a moving car have?", new[] { "Kinetic energy", "Potential energy", "Sound energy" }, 0, "Moving objects have kinetic energy."),
            new Question(3424, "Which material will best reduce friction between two surfaces?", new[] { "Oil", "Sandpaper", "Rope" }, 0, "Oil reduces friction."),
            new Question(3425, "What happens to the length of a shadow when the Sun is high in the sky?", new[] { "It gets shorter", "It gets longer", "It disappears completely" }, 0, "Shadows are shortest when the Sun is high."),
            new Question(3426, "Which planet is closest to the Sun?", new[] { "Mercury", "Venus", "Earth" }, 0, "Mercury is closest to the Sun."),
            new Question(3427, "Which layer of the atmosphere do we live and breathe in?", new[] { "Troposphere", "Stratosphere", "Thermosphere" }, 0, "We live in the troposphere."),
            new Question(3428, "Which part of a circuit provides energy to make current flow?", new[] { "Battery", "Switch", "Bulb" }, 0, "The battery provides energy."),
            new Question(3429, "If a circuit is open, what happens to the bulb?", new[] { "It turns off", "It gets brighter", "It flickers quickly" }, 0, "An open circuit stops current, so it turns off."),
            new Question(3430, "Which symbol shows that two lines are perpendicular?", new[] { "A right angle sign", "An equal sign", "A plus sign" }, 0, "Perpendicular lines form a right angle."),
            new Question(3431, "What is 3/8 as a decimal?", new[] { "0.375", "0.38", "0.83" }, 0, "3 divided by 8 is 0.375."),
            new Question(3432, "What is the value of 5/6 of 30?", new[] { "25", "20", "15" }, 0, "5/6 of 30 equals 25."),
            new Question(3433, "A line of symmetry divides a shape into:", new[] { "Two matching halves", "Four equal parts", "Three angles" }, 0, "It makes two matching halves."),
            new Question(3434, "Which coordinate is written correctly?", new[] { "(3, 5)", "3, 5", "[3-5]" }, 0, "Coordinates are written like (3, 5)."),
            new Question(3435, "What is the perimeter of a square with side length 9 cm?", new[] { "36 cm", "18 cm", "27 cm" }, 0, "Perimeter is 4 × 9 = 36 cm."),
            new Question(3436, "Which continent is the largest by land area?", new[] { "Asia", "Africa", "Europe" }, 0, "Asia is the largest continent."),
            new Question(3437, "On which continent is the Sahara Desert found?", new[] { "Africa", "Asia", "Australia" }, 0, "The Sahara is in Africa."),
            new Question(3438, "Which is often listed as the longest river in the world?", new[] { "Nile River", "Amazon River", "Yangtze River" }, 0, "The Nile is often called the longest."),
            new Question(3439, "Which part of government makes laws in many countries?", new[] { "Legislative branch", "Judicial branch", "Executive branch" }, 0, "The legislative branch makes laws."),
            new Question(3440, "Which scientist is famous for the theory of relativity?", new[] { "Albert Einstein", "Isaac Newton", "Galileo Galilei" }, 0, "Einstein proposed the theory of relativity."),
            new Question(3441, "Which scientist studied motion and used telescopes to observe space?", new[] { "Galileo Galilei", "Marie Curie", "Charles Darwin" }, 0, "Galileo used telescopes to study space."),
            new Question(3442, "Which inventor is linked with improving the electric light bulb?", new[] { "Thomas Edison", "Alexander Graham Bell", "Nikola Tesla" }, 0, "Thomas Edison improved the light bulb."),
            new Question(3443, "Which diagram shows how food energy moves from one organism to another?", new[] { "Food chain", "Bar graph", "Weather map" }, 0, "A food chain shows how energy moves."),
            new Question(3444, "Which of these is a decomposer?", new[] { "Mushroom", "Eagle", "Shark" }, 0, "Mushrooms break down dead things."),
            new Question(3445, "In a food chain, which organisms usually make their own food?", new[] { "Producers", "Consumers", "Decomposers" }, 0, "Producers, like plants, make their own food."),
            new Question(3446, "Which cloud type is thin and wispy and found high in the sky?", new[] { "Cirrus", "Cumulus", "Stratus" }, 0, "Cirrus clouds are thin and high."),
            new Question(3447, "Which of these is measured by a thermometer?", new[] { "Temperature", "Wind speed", "Rainfall" }, 0, "A thermometer measures temperature."),
            new Question(3448, "Which instrument is used to measure wind speed?", new[] { "Anemometer", "Barometer", "Thermometer" }, 0, "An anemometer measures wind speed."),
            new Question(3449, "A scientist who studies rocks is called a:", new[] { "Geologist", "Biologist", "Astronomer" }, 0, "A geologist studies rocks."),
            new Question(3450, "A scientist who studies stars and planets is called a:", new[] { "Astronomer", "Chemist", "Botanist" }, 0, "An astronomer studies space."),
            new Question(3451, "What is 4.5 + 3.2?", new[] { "7.7", "8.5", "6.7" }, 0, "4.5 plus 3.2 equals 7.7."),
            new Question(3452, "What is 9.6 - 4.3?", new[] { "5.3", "6.3", "4.7" }, 0, "9.6 minus 4.3 equals 5.3."),
            new Question(3453, "What is 12 × 12?", new[] { "144", "124", "132" }, 0, "12 times 12 is 144."),
            new Question(3454, "What is 225 ÷ 9?", new[] { "25", "20", "18" }, 0, "225 divided by 9 equals 25."),
            new Question(3455, "Which of these is a ratio?", new[] { "3:4", "3 + 4", "3 × 4" }, 0, "3:4 is a way to compare two quantities."),
            new Question(3456, "If the scale of a map is 1 cm : 5 km, how many kilometers is 3 cm?", new[] { "15 km", "10 km", "5 km" }, 0, "3 × 5 km = 15 km."),
            new Question(3457, "What is 20% of 150?", new[] { "30", "20", "15" }, 0, "20% of 150 is 0.2 × 150 = 30."),
            new Question(3458, "Which is the largest value?", new[] { "0.9", "0.75", "0.5" }, 0, "0.9 is the greatest."),
            new Question(3459, "Which point is farthest from zero on the number line?", new[] { "-10", "-5", "7" }, 0, "7 is farthest to the right."),
            new Question(3460, "What is the probability of rolling a 3 on a fair six-sided die?", new[] { "1/6", "1/3", "1/2" }, 0, "There are 6 sides, so 1 out of 6."),
            new Question(3461, "A data set has values 2, 4, 6, 8, 10. What is the range?", new[] { "8", "10", "12" }, 0, "Range = largest - smallest = 10 - 2 = 8."),
            new Question(3462, "Which of these is an independent clause?", new[] { "She finished her homework.", "When she finished her homework", "Because she finished her homework" }, 0, "It can stand alone as a sentence."),
            new Question(3463, "Which sentence is in passive voice?", new[] { "The cake was baked by Maria.", "Maria baked the cake.", "Maria likes cake." }, 0, "The focus is on 'cake' being baked."),
            new Question(3464, "Which of these is an example of alliteration?", new[] { "Silly snakes slither silently.", "Snakes slither.", "Snakes are long." }, 0, "Alliteration repeats beginning sounds."),
            new Question(3465, "Which genre is most likely to have dragons and magic?", new[] { "Fantasy", "Biography", "Newspaper article" }, 0, "Fantasy often has magic and dragons."),
            new Question(3466, "Which text feature helps you find the meaning of a difficult word in a book?", new[] { "Glossary", "Index", "Table of contents" }, 0, "A glossary explains words."),
            new Question(3467, "Which part of a story introduces the characters and setting?", new[] { "Beginning", "Climax", "Resolution" }, 0, "The beginning introduces them."),
            new Question(3468, "Which organelle is known as the 'powerhouse of the cell'?", new[] { "Mitochondria", "Nucleus", "Cell membrane" }, 0, "Mitochondria release energy."),
            new Question(3469, "Which organelle controls most activities in a cell?", new[] { "Nucleus", "Ribosome", "Vacuole" }, 0, "The nucleus controls cell activities."),
            new Question(3470, "Which gas do humans breathe out more of than they breathe in?", new[] { "Carbon dioxide", "Oxygen", "Hydrogen" }, 0, "We breathe out more carbon dioxide."),
            new Question(3471, "What type of simple machine is a ramp?", new[] { "Inclined plane", "Pulley", "Wedge" }, 0, "A ramp is an inclined plane."),
            new Question(3472, "What happens to particles when a solid melts?", new[] { "They move faster and spread out", "They stop moving", "They disappear" }, 0, "They move faster and are less tightly packed."),
            new Question(3473, "Which of these shows potential energy?", new[] { "A book on a shelf", "A rolling ball", "A flying bird" }, 0, "The book can fall, so it has potential energy."),
            new Question(3474, "Which of these best describes a scientist?", new[] { "Someone who asks questions and tests ideas", "Someone who only memorizes facts", "Someone who never changes their ideas" }, 0, "Scientists ask questions and test ideas."),
            new Question(3475, "Which graph is best for showing parts of a whole?", new[] { "Pie chart", "Line graph", "Pictograph" }, 0, "Pie charts show parts of a whole circle."),
            new Question(3476, "Which disaster would a meteorologist most likely study?", new[] { "Typhoon", "Earthquake", "Volcanic eruption" }, 0, "Meteorologists study weather, like typhoons."),
            new Question(3477, "Which boundary between plates can cause earthquakes?", new[] { "Where plates slide past each other", "Where plates never move", "Where plates are not touching" }, 0, "Sliding plates can cause earthquakes."),
            new Question(3478, "Which of these is a renewable natural resource?", new[] { "Sunlight", "Coal", "Natural gas" }, 0, "Sunlight is renewed every day."),
            new Question(3479, "Which step of the scientific method usually comes first?", new[] { "Ask a question", "Draw a conclusion", "Analyze data" }, 0, "You first ask a question."),
            new Question(3480, "Which is the best example of a fair test in an experiment?", new[] { "Changing only one variable", "Changing many things at once", "Not measuring anything" }, 0, "A fair test changes only one variable at a time."),
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
