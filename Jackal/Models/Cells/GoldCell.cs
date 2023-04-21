using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class GoldCell : Cell
    {
        public GoldCell(int row, int column, Gold goldType) : base(row, column, goldType.ToString())
        {
            _goldType = goldType;
        }

        readonly Gold _goldType;

        public override void Open()
        {
            base.Open();
            if (_goldType == Models.Gold.Galeon)
                Galeon = true;
            else
                Gold = (int)_goldType;
        }
    }
}
