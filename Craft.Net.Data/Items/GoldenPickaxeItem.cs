namespace Craft.Net.Data.Items
{

    public class GoldenPickaxeItem : PickaxeItem
    {
        public override ushort Id
        {
            get
            {
                return 285;
            }
        }

        public override int AttackDamage
        {
            get { return 2; }
        }

        public override ToolMaterial ToolMaterial
        {
            get { return ToolMaterial.Gold; }
        }
    }
}