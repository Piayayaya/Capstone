using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "CharacterCatalog", menuName = "BrainyMe/CharacterCatalog")]
public class CharacterCatalog : ScriptableObject
{
    public List<CharacterDefinition> characters = new();
}
