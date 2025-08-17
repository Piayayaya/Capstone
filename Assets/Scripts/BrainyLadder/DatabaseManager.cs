using System.Collections.Generic;
using UnityEngine;
using Mono.Data.Sqlite;
using System.Data;
using System.IO;

public class DatabaseManager : MonoBehaviour
{
    public static DatabaseManager Instance { get; private set; }
    private string dbPath;

    private void Awake()
    {
        // Setup singleton
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // Define the database file path
        dbPath = Path.Combine(Application.persistentDataPath, "TriviaGameDatabase.db");

        // If the database file doesn't exist, create it and insert sample questions
        if (!File.Exists(dbPath))
        {
            CreateDatabase();
            InsertSampleQuestions();
            CreatePlayerTableIfNeeded();
        }
    }

    void CreateDatabase()
    {
        using (var connection = new SqliteConnection("URI=file:" + dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Create table for quiz-like game modes
                command.CommandText = @"
                    CREATE TABLE IF NOT EXISTS QuestionsTable (
                        Id INTEGER PRIMARY KEY AUTOINCREMENT,
                        QuestionText TEXT NOT NULL,
                        ChoiceA TEXT NOT NULL,
                        ChoiceB TEXT NOT NULL,
                        ChoiceC TEXT NOT NULL,
                        AnswerIndex INTEGER NOT NULL,
                        GameMode TEXT NOT NULL
                    );
                ";
                command.ExecuteNonQuery();
            }
        }
        Debug.Log("Database created and table initialized successfully.");
    }

    void InsertSampleQuestions()
    {
        using (var connection = new SqliteConnection("URI=file:" + dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Insert sample questions for Brainy Ladder mode
                command.CommandText = @"
                    INSERT INTO QuestionsTable (QuestionText, ChoiceA, ChoiceB, ChoiceC, AnswerIndex, GameMode)
                    VALUES 
                    ('What is the capital of the Philippines?', 'Manila', 'Cebu', 'Davao', 0, 'BrainyLadder'),
                    ('What is 5 + 7?', '10', '11', '12', 2, 'BrainyLadder'),
                    ('What is 5 + 6?', '10', '11', '12', 1, 'BrainyLadder'),
                    ('What is 5 + 5?', '10', '11', '12', 0, 'BrainyLadder'),
                    ('Which planet is known as the Red Planet?', 'Mars', 'Earth', 'Venus', 0, 'BrainyLadder');
                ";
                command.ExecuteNonQuery();
            }
        }
        Debug.Log("Sample questions inserted successfully.");
    }

    /// <summary>
    /// Loads questions for the specified game mode (e.g., 'BrainyLadder') from the database.
    /// </summary>
    /// <param name="gameMode">The game mode identifier used in the database.</param>
    /// <returns>A List of Question objects.</returns>
    public List<Question> GetQuestions(string gameMode)
    {
        List<Question> questions = new List<Question>();
        using (var connection = new SqliteConnection("URI=file:" + dbPath))
        {
            connection.Open();
            using (var command = connection.CreateCommand())
            {
                // Fetch questions for the given game mode
                command.CommandText = $"SELECT QuestionText, ChoiceA, ChoiceB, ChoiceC, AnswerIndex FROM QuestionsTable WHERE GameMode = '{gameMode}'";
                using (var reader = command.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        string qText = reader.GetString(0);
                        string[] choices = new string[] { reader.GetString(1), reader.GetString(2), reader.GetString(3) };
                        int answerIndex = reader.GetInt32(4);
                        questions.Add(new Question(qText, choices, answerIndex));
                    }
                }
            }
        }
        Debug.Log("Loaded " + questions.Count + " questions for game mode: " + gameMode);
        return questions;
    }

    private void CreatePlayerTableIfNeeded()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "CREATE TABLE IF NOT EXISTS Player (" +
                                      "ID INTEGER PRIMARY KEY AUTOINCREMENT, " +
                                      "Username TEXT NOT NULL, " +
                                      "Level INTEGER NOT NULL, " +
                                      "CurrentEXP INTEGER NOT NULL);";
                command.ExecuteNonQuery();
            }
        }
    }

    public void InitializePlayerData(string playerName)
    {
        using (var connection = new SqliteConnection("URI=file:" + dbPath))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                // Check if the player data already exists
                command.CommandText = "SELECT COUNT(*) FROM Player WHERE ID = 1";
                long count = (long)command.ExecuteScalar();

                if (count == 0)
                {
                    // Insert base data (Level 1, 0 EXP) with player name if not existing
                    command.CommandText = "INSERT INTO Player (ID, Name, Level, CurrentEXP) VALUES (1, @Name, 1, 0)";
                    command.Parameters.AddWithValue("@Name", playerName);
                    command.ExecuteNonQuery();
                    Debug.Log($"? Player data initialized: {playerName} - Level 1, 0 EXP");
                }
                else
                {
                    Debug.Log("? Player data already exists.");
                }
            }
        }
    }


    public void SaveProgress(string playerName, int level, int currentEXP)
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "INSERT OR REPLACE INTO Player (ID, Name, Level, CurrentEXP) " +
                                      "VALUES (1, @name, @level, @currentEXP);";
                command.Parameters.AddWithValue("@name", playerName);
                command.Parameters.AddWithValue("@level", level);
                command.Parameters.AddWithValue("@currentEXP", currentEXP);
                command.ExecuteNonQuery();
            }
        }
    }

    public (string playerName, int level, int currentEXP) LoadProgress()
    {
        using (var connection = new SqliteConnection(dbPath))
        {
            connection.Open();

            using (var command = connection.CreateCommand())
            {
                command.CommandText = "SELECT Name, Level, CurrentEXP FROM Player WHERE ID = 1;";
                using (var reader = command.ExecuteReader())
                {
                    if (reader.Read())
                    {
                        string playerName = reader.GetString(0);
                        int level = reader.GetInt32(1);
                        int currentEXP = reader.GetInt32(2);
                        return (playerName, level, currentEXP);
                    }
                }
            }
        }

        // Return default values if no progress found
        return ("New Player", 1, 0);
    }

}
