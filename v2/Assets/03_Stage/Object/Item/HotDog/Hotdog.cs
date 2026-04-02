namespace PupSurvivors.Stage.Item
{
    public class Hotdog : ItemBase
    {
        public override void Apply(Player target)
        {
            // todo target에게 피 채우기
            Destroy(gameObject);
        }
    }
}