using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class EarthQuakeCell : Cell
    {
        public EarthQuakeCell(int row, int column, Action startEarthQuake) : base(row, column, "Earthquake")
        {
            _startEarthQuake = startEarthQuake;
        }

        readonly Action _startEarthQuake;

        public override void Open()
        {
            base.Open();
            _startEarthQuake();
        }
    }
}
