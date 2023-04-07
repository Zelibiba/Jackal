using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class HorseCell : Cell
    {
        public HorseCell(int row,int column) : base(row, column, "Horse", false)
        {
        }

        public override void SetSelectableCoords(ObservableMap map)
        {

        }
    }
}
