using System;
using System.Collections.Generic;

[Serializable]
public class AchievementProgressData
{
    public string id;
    public int value;
    public bool completed;
    public string completedAtIso; // optional
    public bool rewardGranted; // set to true after claim
}

[Serializable]
public class AchievementsSave
{
    public List<AchievementProgressData> list = new();

    public AchievementProgressData GetOrCreate(string id)
    {
        var found = list.Find(x => x.id == id);
        if (found == null)
        {
            found = new AchievementProgressData { id = id, value = 0, completed = false };
            list.Add(found);
        }
        return found;
    }
}
