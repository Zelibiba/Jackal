using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class CrocodileCell : Cell
    {
        public CrocodileCell (int row, int column, Predicate<int[]> continueMove):base(row,column,"Crocodile",false)
        {
            _continueMove = continueMove;
        }

        readonly Predicate<int[]> _continueMove;

        public override bool AddPirate(Pirate pirate)
        {
            int[] coords = pirate.Cell.Coords;
            base.AddPirate(pirate);
            return _continueMove(coords);
        }
    }
}
