using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "QuestCatalog", menuName = "BrainyMe/Quests/Catalog")]
public class QuestCatalog : ScriptableObject
{
    public List<QuestDefinition> quests = new List<QuestDefinition>();
}
