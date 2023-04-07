using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackal.Models.Pirates;

namespace Jackal.Models.Cells
{
    public class ShipCell : Cell
    {
        public ShipCell(int row, int column, Team team, (Orientation, int[][]) shipRegion) : base(row, column, "Ship")
        {
            IsOpened = true;
            ShipTeam = team;
            Orientation = shipRegion.Item1;
            _shipRegion = shipRegion.Item2;
            MovableCoords = new List<int[]>();

            if (!(Orientation.Up | Orientation.Right | Orientation.Down | Orientation.Left).HasFlag(Orientation))
                throw new ArgumentException();
        }

        public readonly Orientation Orientation;
        readonly int[][] _shipRegion;
        public List<int[]> MovableCoords;

        public override bool IsShip => true;
        public override Team ShipTeam { get; }
        public bool CanMove => (from Pirate pirate in Pirates
                                where pirate.CanDriveShip
                                select pirate).Count() > 0;


        public override void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();

            int r = Row;
            int c = Column;
            switch (Orientation)
            {
                case Orientation.Up:
                    r--; break;
                case Orientation.Down:
                    r++; break;
                case Orientation.Left:
                    c++; break;
                case Orientation.Right:
                    c--; break;
            }
            SelectableCoords.Add(new int[] { r, c });

            int coord = -1;
            int[] shipCoord = new int[2] { Row, Column };
            switch (Orientation)
            {
                case Orientation.Up:
                case Orientation.Down:
                    coord = 1; break;
                case Orientation.Right:
                case Orientation.Left:
                    coord = 0; break;
            }
            MovableCoords.Clear();
            foreach (int[] coords in _shipRegion)
            {
                if (Math.Abs(coords[coord] - shipCoord[coord]) == 1)
                    MovableCoords.Add(coords);
            }
        }
    }
}
