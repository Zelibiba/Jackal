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

        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            MovementResult result;
            if (IsOpened)
            {
                result = MovementResult.End;
                enterSound = Sounds.Usual;
            }
            else
            {
                result = MovementResult.Cannabis;
                enterSound = Sounds.Cannabis;
            }
            base.AddPirate(pirate, delay);
            return result;
        }
    }
}
