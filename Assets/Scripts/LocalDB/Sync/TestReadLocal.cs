using UnityEngine;
using System.Linq;
using System.Threading.Tasks;

public class TestReadLocal : MonoBehaviour
{
    private async void Start()
    {
        while (!MasterSqliteSync.IsMasterSynced)
            await Task.Yield();

        var advSmart = LocalDb.DB.Table<LocalQuestion>()
            .Where(q => q.gameMode_id == "smartladder" && q.difficulty == "advanced")
            .Count();

        var modes = LocalDb.DB.Table<LocalGamemode>().Count();

        Debug.Log($"[LocalTest AFTER SYNC] advanced smartladder = {advSmart}, gamemodes = {modes}");
        Debug.Log($"[LocalTest] MasterSqliteSync reported modes={MasterSqliteSync.LastGamemodeCount}, questions={MasterSqliteSync.LastQuestionCount}");
    }
}
