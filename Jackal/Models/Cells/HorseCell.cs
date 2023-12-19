using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class HorseCell : Cell
    {
        public HorseCell(int row, int column) : base(row, column, "Horse", false)
        { }

        /// <summary>
        /// Массив относительных координат хода конём.
        /// </summary>
        public static Coordinates[] HorseCoordsPattern => Map.Type == MapType.Quadratic ? _HorseCoordsPatternQuad : _HorseCoordsPatternHex;
        static readonly Coordinates[] _HorseCoordsPatternQuad = new Coordinates[8]
        {
            new(-2,-1),
            new(-1,-2),
            new( 1,-2),
            new( 2,-1),
            new( 2, 1),
            new( 1, 2),
            new(-1, 2),
            new(-2, 1)
        };
        static readonly Coordinates[] _HorseCoordsPatternHex = new Coordinates[6]
        {
            new(-2, 1),
            new(-1,-1),
            new(+1,-2),
            new(+2,-1),
            new(+1, 1),
            new(-1, 2)
        };

        public override void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            SelectableCoords.AddRange(map.AdjacentCellsCoords(this, HorseCoordsPattern));
        }
    }
}
