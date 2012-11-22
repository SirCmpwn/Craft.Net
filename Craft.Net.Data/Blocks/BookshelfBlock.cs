using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Craft.Net.Data.Items;

namespace Craft.Net.Data.Blocks
{
   public class BookshelfBlock : Block
   {
      public override ushort Id
      {
         get { return 47; }
      }

      public override double Hardness
      {
         get { return 1.5; }
      }

      public override bool GetDrop(ToolItem tool, out Slot[] drops)
      {
         drops = new[] { new Slot((ushort)new BookItem(), 3) };
         return true;
      }
   }
}