using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class SeaCell : Cell
    {
        public SeaCell(int row, int column) : base(row, column, "Sea")
        {
            Open();
        }

        public override int Gold
        {
            get => 0;
            set => Game.LostGold++;
        }
        public override bool Galeon
        {
            get => false;
            set => Game.LostGold += 3;
        }

        public override MovementResult AddPirate(Pirate pirate)
        {
            MovementResult result = base.AddPirate(pirate);
            pirate.Gold = false;
            pirate.Galeon = false;
            return result;
        }

        public override void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            foreach(Coordinates coords in map.AdjacentCellsCoords(this))
            {
                if (map[coords] is not SeaCell && map[coords] is not ShipCell) continue;
                SelectableCoords.Add(coords);
            }
        }
    }
}
