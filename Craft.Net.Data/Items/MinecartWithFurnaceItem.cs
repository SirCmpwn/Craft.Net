namespace Craft.Net.Data.Items
{
    
    public class MinecartWithFurnaceItem : Item
    {
        public override ushort Id
        {
            get
            {
                return 343;
            }
        }

        public override byte MaximumStack
        {
            get { return 1; }
        }
    }
}
