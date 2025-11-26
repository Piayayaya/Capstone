using UnityEngine;
using SQLite4Unity3d;
using System.IO;
using System.Linq;

public class SqliteSmokeTest : MonoBehaviour
{
    void Start()
    {
        var path = Path.Combine(Application.persistentDataPath, "smoke.db");
        var conn = new SQLiteConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create);

        // ✅ Create table based on class name
        conn.CreateTable<TestRow>();

        conn.Insert(new TestRow { name = "hello" });

        var rows = conn.Table<TestRow>().ToList();
        Debug.Log("SQLite OK. Rows = " + rows.Count);
    }
}

// ✅ Make it a top-level class (not nested) to avoid reflection quirks
public class TestRow
{
    [PrimaryKey, AutoIncrement] public int id { get; set; }
    public string name { get; set; }
}
