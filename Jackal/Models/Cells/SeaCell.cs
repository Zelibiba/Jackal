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

        public override void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            SelectableCoords.AddRange(map.AdjacentCellsCoords(this).Where(coords => map[coords] is SeaCell or ShipCell));
        }

        public override bool CanBeSelectedBy(Pirate pirate)
        {
            return pirate.Cell is SeaCell || !pirate.Cell.IsStandable;
        }
        public override bool IsGoldFriendly(Pirate pirate)
        {
            return !pirate.Cell.IsStandable;
        }

        public override MovementResult AddPirate(Pirate pirate, int delay =0)
        {
            MovementResult result = base.AddPirate(pirate, delay);
            pirate.Gold = false;
            pirate.Galeon = false;
            return result;
        }
    }
}
