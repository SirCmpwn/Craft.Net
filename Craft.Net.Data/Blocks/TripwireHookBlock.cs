using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data.Blocks
{
    public class TripwireHookBlock : Block
    {
        public override ushort Id
        {
            get { return 131; }
        }

        public override BoundingBox? BoundingBox
        {
            get { return null; }
        }

        public override bool OnBlockPlaced(World world, Vector3 position, Vector3 clickedBlock, Vector3 clickedSide, Vector3 cursorPosition, Entities.Entity usedBy)
        {
            this.Metadata = (byte)DataUtility.DirectionByRotationFlat(usedBy); // TODO
            return true;
        }
    }
}