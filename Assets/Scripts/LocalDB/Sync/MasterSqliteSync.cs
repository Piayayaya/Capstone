using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using Firebase;
using Firebase.Database;
using Firebase.Extensions;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Linq;

public class MasterSqliteSync : MonoBehaviour
{
    public static bool IsMasterSynced = false;
    public static int LastQuestionCount = 0;
    public static int LastGamemodeCount = 0;

    private DatabaseReference root;
    private class RemoteQuestRow
    {
        public string questId;
        public string title;
        public string description;
        public string progressTag;
        public int target;
        public int coinReward;

        public string createdAt;
        public string updatedAt;
        public string createdBy;
        public string updatedBy;
    }

    private class RemoteAchievementRow
    {
        public string achievementId;
        public string title;
        public string description;
        public string progressTag;
        public int target;
        public int coinReward;
        public string iconId;

        public string createdAt;
        public string updatedAt;
        public string createdBy;
        public string updatedBy;
    }

    private int SaveAchievementsToSqlite(string achievementsJson)
    {
        if (string.IsNullOrWhiteSpace(achievementsJson))
        {
            Debug.LogWarning("[MasterSqliteSync] Achievements JSON empty/null.");
            return 0;
        }

        Dictionary<string, RemoteAchievementRow> map = null;

        try
        {
            map = JsonConvert.DeserializeObject<Dictionary<string, RemoteAchievementRow>>(achievementsJson);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[MasterSqliteSync] Failed to parse achievementsJson: " + ex);
            return 0;
        }

        if (map == null || map.Count == 0)
        {
            Debug.LogWarning("[MasterSqliteSync] No achievements parsed from JSON.");
            return 0;
        }

        var list = new List<LocalAchievementDef>();

        foreach (var kv in map)
        {
            var r = kv.Value;
            if (r == null) continue;

            string id = string.IsNullOrEmpty(r.achievementId) ? kv.Key : r.achievementId;

            var local = new LocalAchievementDef
            {
                id = id,
                title = r.title ?? "",
                description = r.description ?? "",
                progressTag = r.progressTag ?? "",
                target = r.target,
                coinReward = r.coinReward,
                iconId = r.iconId ?? "",

                createdAt = r.createdAt ?? "",
                updatedAt = r.updatedAt ?? "",
                createdBy = r.createdBy ?? "",
                updatedBy = r.updatedBy ?? ""
            };

            list.Add(local);
        }

        LocalDb.DB.RunInTransaction(() =>
        {
            LocalDb.DB.DeleteAll<LocalAchievementDef>();
            LocalDb.DB.InsertAll(list);
        });

        Debug.Log($"[MasterSqliteSync] Saved {list.Count} achievements to SQLite.");
        return list.Count;
    }


    private void SaveQuestsToSqlite(string questsJson)
    {
        if (string.IsNullOrWhiteSpace(questsJson))
        {
            Debug.LogWarning("[MasterSqliteSync] Quests JSON empty/null.");
            return;
        }

        // Firebase tblQuests is an object keyed by questId
        // { "q_answers_10_any": { questId: "...", title: "...", ... }, ... }
        Dictionary<string, RemoteQuestRow> map = null;

        try
        {
            map = JsonConvert.DeserializeObject<Dictionary<string, RemoteQuestRow>>(questsJson);
        }
        catch (System.Exception ex)
        {
            Debug.LogError("[MasterSqliteSync] Failed to parse questsJson: " + ex);
            return;
        }

        if (map == null || map.Count == 0)
        {
            Debug.LogWarning("[MasterSqliteSync] No quests parsed from JSON.");
            return;
        }

        var list = new List<LocalQuestDef>();

        foreach (var kv in map)
        {
            var r = kv.Value;
            if (r == null) continue;

            string id = string.IsNullOrEmpty(r.questId) ? kv.Key : r.questId;

            var local = new LocalQuestDef
            {
                id = id,
                title = r.title ?? "",
                description = r.description ?? "",
                progressTag = r.progressTag ?? "",
                target = r.target,
                coinReward = r.coinReward,
                createdAt = r.createdAt ?? "",
                updatedAt = r.updatedAt ?? "",
                createdBy = r.createdBy ?? "",
                updatedBy = r.updatedBy ?? ""
            };

            list.Add(local);
        }

        LocalDb.DB.RunInTransaction(() =>
        {
            LocalDb.DB.DeleteAll<LocalQuestDef>();
            LocalDb.DB.InsertAll(list);
        });

        Debug.Log($"[MasterSqliteSync] Saved {list.Count} quests to SQLite.");
    }

    private void Awake()
    {
        DontDestroyOnLoad(gameObject);

        LocalDb.Init();

        Debug.Log("[MasterSqliteSync] Awake started");
        InitFirebaseAndSync();
    }

    private void InitFirebaseAndSync()
    {
        Debug.Log("[MasterSqliteSync] Checking Firebase dependencies...");

        FirebaseApp.CheckAndFixDependenciesAsync()
            .ContinueWithOnMainThread(async task =>
            {
                if (task.Result != DependencyStatus.Available)
                {
                    Debug.LogError("[MasterSqliteSync] Firebase deps NOT available: " + task.Result);
                    return;
                }

                // Set DB URL here so this script is independent
                FirebaseApp.DefaultInstance.Options.DatabaseUrl =
                    new System.Uri("https://brainyme-firebase-default-rtdb.asia-southeast1.firebasedatabase.app/");

                root = FirebaseDatabase.DefaultInstance.RootReference;

                Debug.Log("[MasterSqliteSync] Firebase READY. Starting master sync...");
                await SyncMaster();
            });
    }

    private async Task SyncMaster()
    {
        if (Application.internetReachability == NetworkReachability.NotReachable)
        {
            Debug.LogWarning("[MasterSqliteSync] Offline, skipping master sync.");
            IsMasterSynced = true; // allow game to proceed using old local data
            return;
        }
        Debug.Log("[MasterSqliteSync] Downloading master data...");

        var modesSnap = await root.Child("Gamemodes").GetValueAsync();
        var qSnap = await root.Child("Questions").GetValueAsync();
        var questsSnap = await root.Child("Quests").GetValueAsync();
        var achSnap = await root.Child("Achievements").GetValueAsync();

        string questsJson = questsSnap.GetRawJsonValue();
        string modesJson = modesSnap.GetRawJsonValue();
        string qJson = qSnap.GetRawJsonValue();
        string achievementsJson = achSnap.GetRawJsonValue();

        Debug.Log("[MasterSqliteSync] Gamemodes json length = " + (modesJson?.Length ?? 0));
        Debug.Log("[MasterSqliteSync] Questions json length = " + (qJson?.Length ?? 0));
        Debug.Log($"[MasterSqliteSync] Quests json length = {questsJson?.Length ?? 0}");
        Debug.Log("[MasterSqliteSync] Achievements json length = " + (achievementsJson?.Length ?? 0));

        LastGamemodeCount = UpsertGamemodes(modesJson);
        LastQuestionCount = UpsertQuestions(qJson);
        SaveQuestsToSqlite(questsJson);
        SaveAchievementsToSqlite(achievementsJson);

        IsMasterSynced = true;

        Debug.Log($"[MasterSqliteSync] Master sync complete! modes={LastGamemodeCount}, questions={LastQuestionCount}");
    }

    // ---------------- GAMEMODES ----------------
    int UpsertGamemodes(string json)
    {
        if (string.IsNullOrEmpty(json)) return 0;

        var rootDict = JsonConvert.DeserializeObject<Dictionary<string, GamemodeDTO>>(json);
        if (rootDict == null) return 0;

        int count = 0;

        LocalDb.DB.RunInTransaction(() =>
        {
            foreach (var pair in rootDict)
            {
                string id = pair.Key;
                var g = pair.Value;
                if (g == null) continue;

                LocalDb.DB.InsertOrReplace(new LocalGamemode
                {
                    id = id,
                    gameModeName = g.gameModeName,
                    gameInstruc = g.gameInstruc,
                    updated_at = g.updatedAt
                });

                count++;
            }
        });

        Debug.Log("[SQLite] Gamemodes upserted: " + count);
        return count;
    }

    // ---------------- QUESTIONS (YOUR 3-LEVEL SHAPE) ----------------
    // Questions/{gameModeId}/{difficulty}/{questionId}:{...}
    int UpsertQuestions(string json)
    {
        if (string.IsNullOrEmpty(json)) return 0;

        Dictionary<string, Dictionary<string, Dictionary<string, QuestionDTO>>> rootDict;

        try
        {
            rootDict = JsonConvert.DeserializeObject<
                Dictionary<string, Dictionary<string, Dictionary<string, QuestionDTO>>>>
                (json);
        }
        catch (System.Exception e)
        {
            Debug.LogError("[SQLite] Failed parsing Questions JSON: " + e.Message);
            return 0;
        }

        if (rootDict == null) return 0;

        int count = 0;

        LocalDb.DB.RunInTransaction(() =>
        {
            foreach (var gmPair in rootDict)
            {
                string gmNode = gmPair.Key; // smartladder
                var diffs = gmPair.Value;
                if (diffs == null) continue;

                foreach (var diffPair in diffs)
                {
                    string diffNode = diffPair.Key; // advanced
                    var questions = diffPair.Value;
                    if (questions == null) continue;

                    foreach (var qPair in questions)
                    {
                        string qId = qPair.Key; // 3301
                        var q = qPair.Value;
                        if (q == null) continue;

                        var choicesList = q.GetChoicesAsList();

                        LocalDb.DB.InsertOrReplace(new LocalQuestion
                        {
                            id = qId,
                            gameMode_id = string.IsNullOrEmpty(q.gameModeId) ? gmNode : q.gameModeId,
                            difficulty = string.IsNullOrEmpty(q.difficulty) ? diffNode : q.difficulty,
                            question = q.question,
                            choicesJson = JsonConvert.SerializeObject(choicesList),
                            correctIndex = q.correctAnsIndex,
                            updated_at = q.updatedAt,
                            explanation = q.explanation
                        });

                        count++;
                    }
                }
            }
        });

        Debug.Log("[SQLite] Questions upserted: " + count);
        return count;
    }
}

#region DTOs

[System.Serializable]
public class GamemodeDTO
{
    public string gameModeName;
    public string gameInstruc;  // if your node uses different name, rename here
    public string updatedAt;
}

[System.Serializable]
public class QuestionDTO
{
    public string question;
    public string difficulty;
    public string gameModeId;

    public JToken choices;
    public int correctAnsIndex;
    public string updatedAt;
    public string explanation; 

    public List<string> GetChoicesAsList()
    {
        var list = new List<string>();
        if (choices == null) return list;

        if (choices.Type == JTokenType.Array)
        {
            foreach (var c in choices) list.Add(c.ToString());
        }
        else if (choices.Type == JTokenType.Object)
        {
            var obj = (JObject)choices;
            foreach (var prop in obj.Properties())
                list.Add(prop.Value.ToString());
        }
        return list;
    }
}

#endregion
