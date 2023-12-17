using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    internal class LakeCell : Cell
    {
        public LakeCell(int row, int column, Func<Coordinates, MovementResult> continueMove) : base(row, column, "Lake", false)
        {
            _continueMove = continueMove;
            _mapCoords = new List<Coordinates>();
            _horseCoords = new List<Coordinates>();
        }

        readonly Func<Coordinates, MovementResult> _continueMove;
        readonly List<Coordinates> _mapCoords;
        readonly List<Coordinates> _horseCoords;

        public override void SetSelectableCoords(Map map)
        {
            _mapCoords.Clear();
            foreach (Cell cell in map)
            {
                if (cell is SeaCell || cell is ShipCell || cell.Coords == Coords) continue;
                _mapCoords.Add(cell.Coords);
            }
            _horseCoords.Clear();
            foreach (Coordinates coords in map.AdjacentCellsCoords(this, HorseCell.HorseCoordsPattern))
                _horseCoords.Add(coords);
        }

        public override MovementResult AddPirate(Pirate pirate, int delay = 0)
        {
            SelectableCoords.Clear();
            if (pirate.AtHorse)
            {
                SelectableCoords.AddRange(_horseCoords);
                return base.AddPirate(pirate, delay);
            }
            else if (pirate.AtAirplane)
            {
                SelectableCoords.AddRange(_mapCoords);
                return base.AddPirate(pirate, delay);
            }
            else
            {
                Coordinates coords = Coords + (Coords - pirate.Cell.Coords);
                base.AddPirate(pirate, delay);
                return _continueMove(coords);
            }
        }
    }
}
