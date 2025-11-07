using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "BrainyMe/Achievements/Catalog")]
public class AchievementsCatalog : ScriptableObject
{
    public List<AchievementDef> items = new();
}
