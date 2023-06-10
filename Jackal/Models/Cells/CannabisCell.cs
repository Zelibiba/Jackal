using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class CannabisCell : Cell
    {
        public CannabisCell(int row, int column) : base(row, column, "Cannabis") { }

        public override MovementResult AddPirate(Pirate pirate)
        {
            MovementResult result = IsOpened ? MovementResult.End : MovementResult.Cannabis;
            base.AddPirate(pirate);
            return result;
        }
    }
}
