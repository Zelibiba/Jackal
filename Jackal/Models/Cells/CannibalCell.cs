using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class CannibalCell : Cell
    {
        public CannibalCell(int row, int column) : base(row, column, "Cannibal")
        { }

        public override bool AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            pirate.Kill();
            return true;
        }
    }
}
