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
        public ShipCell(int row, int column, Player owner, (Orientation, int[][]) shipRegion) : base(row, column, "Ship")
        {
            IsOpened = true;
            _owner = owner;
            Manager = owner;
            _owner.SetShip(this);

            Orientation = shipRegion.Item1;
            _shipRegion = shipRegion.Item2;
            MovableCoords = new List<int[]>();

            if (!(Orientation.Up | Orientation.Right | Orientation.Down | Orientation.Left).HasFlag(Orientation))
                throw new ArgumentException("Wrong ship orientation!");
        }

        /// <summary>
        /// Направление выхода с корабля.
        /// </summary>
        readonly public Orientation Orientation;
        readonly int[][] _shipRegion;
        /// <summary>
        /// Координаты, на которые корабль может переместиться.
        /// </summary>
        readonly public List<int[]> MovableCoords;

        public override bool IsShip => true;
        readonly Player _owner;
        public Player Manager { get; private set; }
        public override Team ShipTeam => _owner.Team;
        /// <summary>
        /// Флаг того, что корабль может перемещаться.
        /// </summary>
        public bool CanMove => Pirates.Any(pirate => pirate.IsFighter);

        public override int Gold
        {
            get => 0;
            set => _owner.Gold++;
        }
        public override bool Galeon
        {
            get => false;
            set => _owner.Gold += 3;
        }

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
            int[] shipCoord = Coords;
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
