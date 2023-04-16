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
        public BalloonCell(int row, int column, Predicate<int[]> continueMove) : base(row, column, "Balloon", false)
        {
            _continueMove = continueMove;
        }

        readonly Predicate<int[]> _continueMove;

        public override bool AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            return _continueMove(pirate.Owner.Ship.Coords);
        }
    }
}
