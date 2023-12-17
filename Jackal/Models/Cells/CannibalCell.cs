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

        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            base.AddPirate(pirate, delay);
            if (pirate is not Friday)
                pirate.Kill();
            return MovementResult.End;
        }
    }
}
