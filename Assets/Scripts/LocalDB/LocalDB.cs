using System.IO;
using UnityEngine;
using SQLite4Unity3d;

public static class LocalDb
{
    private static SQLiteConnection _db;

    public static SQLiteConnection DB
    {
        get
        {
            if (_db == null) Init();
            return _db;
        }
    }

    public static void Init()
    {
        var path = Path.Combine(Application.persistentDataPath, "brainyme_master.db");
        _db = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        // Master tables
        _db.CreateTable<LocalGamemode>();
        _db.CreateTable<LocalQuestion>();
        DB.CreateTable<LocalQuestDef>();
        DB.CreateTable<LocalAchievementDef>();

        Debug.Log("[LocalDb] SQLite ready: " + path);
    }
}
