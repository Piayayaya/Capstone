using SQLite4Unity3d;

[Table("Questions")]
public class LocalQuestion
{
    [PrimaryKey] public string id { get; set; }   // Firebase questionId (ex "3301")

    public string gameMode_id { get; set; }       // "smartladder"
    public string difficulty { get; set; }        // "advanced"

    public string question { get; set; }          // question text
    public string choicesJson { get; set; }       // choices stored as JSON array string
    public int correctIndex { get; set; }         // correctAnsIndex from Firebase

    public string updated_at { get; set; }        // optional

    public string explanation { get; set; }

}
