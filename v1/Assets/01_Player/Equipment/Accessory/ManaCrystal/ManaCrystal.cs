namespace PupSurvivors.Equipment
{
    public class ManaCrystal : AccessoryBase
    {
        // 레벨 올릴 시 SP 꽉 채워줌
        public override void LevelUp()
        {
            base.LevelUp();
            PlayerController.Instance.SetSpFull();
        }
    }
}