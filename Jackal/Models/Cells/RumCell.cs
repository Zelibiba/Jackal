using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class RumCell : Cell
    {
        public RumCell(int row,int column) : base(row, column, "Rum") { }

        public override MovementResult AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);

            if (pirate is Friday)
                pirate.Kill();
            else if (pirate is Missioner missioner)
                missioner.ConverToPirate();
            else
            {
                pirate.RumCount = 2;
                pirate.IsBlocked = true;
                pirate.IsEnabled = false;
            }

            return MovementResult.End;
        }
    }
}
