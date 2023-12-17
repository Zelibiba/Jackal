using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class EarthQuakeCell : Cell
    {
        public EarthQuakeCell(int row, int column) : base(row, column, "Earthquake") { }

        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            MovementResult result = IsOpened ? MovementResult.End : MovementResult.EarthQuake;
            base.AddPirate(pirate, delay);
            return result;
        }
    }
}
