using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class ShipCell : Cell
    {
        public ShipCell(int row, int column, Team team) : base(row, column, "Ship")
        {
            IsOpened = true;
            ShipTeam = team;
        }

        public override bool IsShip => true;
        public override Team ShipTeam { get; }

        public override void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            SelectableCoords.Add(new int[] { row + 1, column });
        }
    }
}
