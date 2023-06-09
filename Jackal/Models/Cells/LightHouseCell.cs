using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class LightHouseCell : Cell
    {
        public LightHouseCell(int row,int column) : base(row, column, "LightHouse") { }

        public override MovementResult AddPirate(Pirate pirate)
        {
            MovementResult result = IsOpened ? MovementResult.End : MovementResult.LightHouse;
            base.AddPirate(pirate);
            return result;
        }
    }
}
