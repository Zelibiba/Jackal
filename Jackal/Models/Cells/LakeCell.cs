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
        public LakeCell(int row, int column, Action<int[]> continueMove) : base(row, column, "Lake", false)
        {
            _continueMove = continueMove;
        }

        int _mapSize;
        Action<int[]> _continueMove;

        public override void SetSelectableCoords(ObservableMap map)
        {
            _mapSize = map.MapSize;
        }

        public override void AddPirate(Pirate pirate)
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
                base.AddPirate(pirate);
            }
            else
            {
                int[] coords = new int[2] { Row + (Row - pirate.Row), Column + (Column - pirate.Column) };
                base.AddPirate(pirate);
                _continueMove(coords);
            }
        }
    }
}
