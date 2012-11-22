using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Craft.Net.Data.Blocks
{
    public class DetectorRailBlock : Block
    {
        public override ushort Id
        {
            get { return 28; }
        }

        public override double Hardness
        {
            get { return 0.7; }
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