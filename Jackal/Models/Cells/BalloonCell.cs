using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class BalloonCell : Cell
    {
        public BalloonCell(int row, int column, Func<Coordinates, MovementResult> continueMove) : base(row, column, "Balloon", false)
        {
            _continueMove = continueMove;
        }

        readonly Func<Coordinates, MovementResult> _continueMove;

        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            base.AddPirate(pirate, delay);
            return _continueMove(pirate.Owner.Ship.Coords);
        }
    }
}
