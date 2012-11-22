using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data.Blocks
{
    public class StonePressurePlateBlock : Block
    {
        public override ushort Id
        {
            get { return 70; }
        }

        public override double Hardness
        {
            get { return 0.5; }
        }

        public override BoundingBox? BoundingBox
        {
            get { return null; }
        }

        public override bool RequiresSupport
        {
            get { return true; }
        }

        public override Vector3 SupportDirection
        {
            get { return Vector3.Down; }
        }
    }
}