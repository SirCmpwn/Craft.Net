namespace Craft.Net.Data.Items
{
    
    public class GoldenBootsItem : Item
    {
        public override ushort Id
        {
            get
            {
                return 317;
            }
        }

        public override bool IsArmor
        {
            get { return true; }
        }

        public override int ArmorBonus
        {
            get { return 1; }
        }
    }
}