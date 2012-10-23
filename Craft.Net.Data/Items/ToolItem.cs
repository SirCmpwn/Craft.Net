using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data.Items
{
    public enum ToolType
    {
        None,
        Pick,
        Axe,
        Shovel,
        Sword,
        Hoe,
        Other
    }

    public enum ToolMaterial
    {
        Wood,
        Stone,
        Iron,
        Gold,
        Diamond,
        Other
    }

    public abstract class ToolItem : Item
    {
        public override byte MaximumStack
        {
            get { return 1; }
        }

        public abstract ToolType ToolType { get; }
        public abstract ToolMaterial ToolMaterial { get; }

        public bool CanHarvest(Block block)
        {
            return block.CanHarvest(this);
        }

        /// <summary>
        /// Returns true if the item is destroyed.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        public bool Damage(int amount)
        {
            if (ToolType == ToolType.Other)
                return false;
            Data += (ushort)amount;
            return Data >= GetIdealUses();
        }

        private ushort GetIdealUses()
        {
            if (ToolType != ToolType.Other)
            {
                switch (ToolMaterial)
                {
                    case ToolMaterial.Wood:
                        return 60;
                    case ToolMaterial.Stone:
                        return 132;
                    case ToolMaterial.Iron:
                        return 251;
                    case ToolMaterial.Gold:
                        return 33;
                    case ToolMaterial.Diamond:
                        return 1562;
                }
            }
            return 0xFFFF;
        }

        public virtual bool IsEfficient(Block block)
        {
            return false;
        }
    }
}
