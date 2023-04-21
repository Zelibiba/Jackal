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
        public LakeCell(int row, int column, Func<int[], MovementResult> continueMove) : base(row, column, "Lake", false)
        {
            _continueMove = continueMove;
            _mapCoords = new List<int[]>();
        }

        readonly Func<int[], MovementResult> _continueMove;
        int _mapSize;
       readonly List<int[]> _mapCoords;

        public override void SetSelectableCoords(ObservableMap map)
        {
            _mapSize = map.MapSize;
            foreach (Cell cell in map)
            {
                if (cell is SeaCell || cell is ShipCell ||
                    cell.HasSameCoords(Row, Column))
                    continue;
                _mapCoords.Add(cell.Coords);
            }
        }

        public override MovementResult AddPirate(Pirate pirate)
        {
            SelectableCoords.Clear();
            if (pirate.AtHorse)
            {
                foreach (int[] coords in HorseCell.SelectableCoordsPattern)
                {
                    int row = Row + coords[0];
                    int column = Column + coords[1];
                    if (row < 0 || column < 0 || row >= _mapSize || column >= _mapSize)
                        continue;
                    SelectableCoords.Add(new int[] { row, column });
                }
                return base.AddPirate(pirate);
            }
            else if(pirate.AtAirplane)
            {
                SelectableCoords.AddRange(_mapCoords);
                return base.AddPirate(pirate);
            }
            else
            {
                int[] coords = new int[2] { Row + (Row - pirate.Row), Column + (Column - pirate.Column) };
                base.AddPirate(pirate);
                return _continueMove(coords);
            }
        }
    }
}
