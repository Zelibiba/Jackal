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
        public static readonly int[][] SelectableCoordsPattern = new int[8][]
        {
            new int[2] { -2,  1 },
            new int[2] { -1,  2 },
            new int[2] {  1,  2 },
            new int[2] {  2,  1 },
            new int[2] {  2, -1 },
            new int[2] {  1, -2 },
            new int[2] { -1, -2 },
            new int[2] { -2, -1 }
        };

        public override void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            foreach (int[] coords in SelectableCoordsPattern)
            {
                int row = Row + coords[0];
                int column = Column + coords[1];
                if (map.CheckIndexes(row, column))
                    continue;
                SelectableCoords.Add(new int[] { row, column });
            }
        }
    }
}
