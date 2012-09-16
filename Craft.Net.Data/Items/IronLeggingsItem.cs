namespace Craft.Net.Data.Items
{
    
    public class IronLeggingsItem : Item
    {
        public override ushort Id
        {
            get
            {
                return 308;
            }
        }

        public override bool IsArmor
        {
            get { return true; }
        }

        public override int ArmorBonus
        {
            get { return 5; }
        }
    }
}