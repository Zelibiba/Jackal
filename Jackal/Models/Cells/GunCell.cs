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
        public GunCell(int row, int column, int rotation, Predicate<int[]> continueMove) : base(row, column, "Gun", false)
        {
            _continueMove = continueMove;
            (this as IOrientable).Orientations.Add(Orientation.Up);
            _angle = (this as IOrientable).Rotate(rotation);
        }

        Predicate<int[]> _continueMove;

        public override int Angle => _angle;
        readonly int _angle;

        List<Orientation> IOrientable.Orientations { get; } = new List<Orientation>();
        Orientation orientation => (this as IOrientable).Orientations[0];

        public override bool AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            return _continueMove(SelectableCoords[0]);
        }

        public override void SetSelectableCoords(ObservableMap map)
        {
            int[] coords = new int[2] { Row, Column };
            int i_changed = (Orientation.Up | Orientation.Down).HasFlag(orientation) ? 0 : 1;
            int changing = (Orientation.Down | Orientation.Right).HasFlag(orientation) ? 1 : -1;
            do
                coords[i_changed] += changing;
            while (map[coords] is not SeaCell && map[coords] is not ShipCell);
            SelectableCoords.Add(coords);
        }
    }
}
