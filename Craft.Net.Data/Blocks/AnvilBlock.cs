using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data.Blocks
{
    public enum AnvilDamage
    {
        Intact = 0,
        SlightlyDamaged = 1,
        VeryDamaged = 2
    }

    public class AnvilBlock : Block
    {
        public override ushort Id
        {
            get { return 145; }
        }

        public override double Hardness
        {
            get { return 5; }
        }

        public AnvilDamage AnvilDamage
        {
            get
            {
                return (AnvilDamage)Metadata;
            }
            set
            {
                Metadata = (byte)value;
            }
        }

        public override bool OnBlockRightClicked(Vector3 clickedBlock, Vector3 clickedSide, Vector3 cursorPosition, World world, Entities.Entity usedBy)
        {
            // TODO: Window
            return false;
        }
    }
}
