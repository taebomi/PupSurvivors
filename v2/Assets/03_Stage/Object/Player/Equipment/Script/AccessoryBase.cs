using Cysharp.Threading.Tasks;
using PupSurvivors.Stage;

namespace PupSurvivors.Equipment
{
    public class AccessoryBase : EquipmentBase
    {
        public AccessoryData Data { get; private set; }
        public override EquipmentData EquipmentData => Data;


        protected override UniTaskVoid Initialize()
        {
            Data = StageManager.Instance.EquipmentDB.equipmentDict[GetType().Name] as AccessoryData;
            return default;
        }


        public override void LevelUp()
        {
            CurLevel++;
        }

        public CharacterStats GetCurLevelData()
        {
            return Data.levelData[CurLevel - 1];
        }
    }
}