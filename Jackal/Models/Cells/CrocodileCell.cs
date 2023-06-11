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
        public CrocodileCell (int row, int column, Func<int[], MovementResult> continueMove):base(row,column,"Crocodile",false)
        {
            _continueMove = continueMove;
        }

        readonly Func<int[], MovementResult> _continueMove;

        public override MovementResult AddPirate(Pirate pirate)
        {
            int[] coords = pirate.Cell.Coords;
            base.AddPirate(pirate);
            if (pirate.IsInLoop)
            {
                pirate.LoopKill();
                return MovementResult.End;
            }
            return _continueMove(coords);
        }
    }
}
