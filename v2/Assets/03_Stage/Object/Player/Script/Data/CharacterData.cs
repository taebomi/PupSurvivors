using UnityEngine;


[CreateAssetMenu(menuName = "PupSurvivors/Character Data", fileName = "CharacterData", order = 1000)]
public class CharacterData : ScriptableObject
{
    public string characterName;
    public string defaultEquipmentName;
    public CharacterStats stats;
}
