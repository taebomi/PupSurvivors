using System;
using PupSurvivors.Stage.Character;

namespace PupSurvivors.System
{
    [ES3Serializable]
    [Serializable]
    public class SaveData
    {
        public CharacterName selectedCharacterName;
        public int maxWeaponNum;
        public int maxAccessoryNum;

        public SaveData()
        {
            selectedCharacterName = CharacterName.BaekGu;
        }
    }
}