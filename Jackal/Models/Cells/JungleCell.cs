using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class JungleCell : Cell
    {
        public JungleCell(int row,int column) : base(row, column, "Jungle") { }

        protected override Team Team => Team.None;

        public override bool IsGoldFriendly(Pirate pirate) => false;
        
    }
}
