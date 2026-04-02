using System;
using System.Collections.Generic;


namespace PupSurvivors.Equipment
{
    public class EquipmentTextData : Dictionary<string, EquipmentTextSet>
    {
    }

    [Serializable]
    public class EquipmentTextSet
    {
        public string name;
        public string[] description;
    }
}