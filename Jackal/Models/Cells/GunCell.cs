using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class GunCell : Cell, IOrientable
    {
        public GunCell(int row, int column, int rotation, Func<int[], MovementResult> continueMove) : base(row, column, "Gun", false)
        {
            _continueMove = continueMove;
            _orientation = Orientation.Up;
            _angle = (this as IOrientable).Rotate(rotation, ref _orientation);
        }

        readonly Func<int[], MovementResult> _continueMove;

        public override int Angle => _angle;
        readonly int _angle;
        Orientation _orientation;

        public override MovementResult AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            return _continueMove(SelectableCoords[0]);
        }

        public override void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            int[] coords = Coords;
            int i_changed = (Orientation.Up | Orientation.Down).HasFlag(_orientation) ? 0 : 1;
            int changing = (Orientation.Down | Orientation.Right).HasFlag(_orientation) ? 1 : -1;
            do
                coords[i_changed] += changing;
            while (map[coords] is not SeaCell && map[coords] is not ShipCell);
            SelectableCoords.Add(coords);
        }
    }
}
