using Jackal.Models.Cells.Utilites;
using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class GunCell : Cell, IOrientableCell
    {
        public GunCell(int row, int column, int rotation, Func<Coordinates, MovementResult> continueMove) : base(row, column, "Gun", false)
        {
            _continueMove = continueMove;
            Directions = new Coordinates[] { new(-1, 0) };
            _angle = (this as IOrientableCell).Rotate(rotation);
        }

        readonly Func<Coordinates, MovementResult> _continueMove;

        public Coordinates[] Directions { get; }
        public override int Angle => _angle;
        readonly int _angle;

        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            base.AddPirate(pirate, delay);
            return _continueMove(SelectableCoords[0]);
        }

        public override void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            Coordinates coords = Coords;
            do
                coords += Directions[0];
            while (!(map[coords] is SeaCell || map[coords] is ShipCell));
            SelectableCoords.Add(coords);
        }
    }
}
