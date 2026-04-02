using System;

namespace PupSurvivors.System
{
    [ES3Serializable]
    [Serializable]
    public class SettingData
    {
        public Language language;

        public SettingData()
        {
            language = Language.Kor;
        }

        public enum Language
        {
            Kor,
            Eng,
        }
    }
}