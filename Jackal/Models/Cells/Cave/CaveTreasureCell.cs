using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells.Cave
{
    public class CaveTreasureCell : Cell
    {
        public CaveTreasureCell(CaveCell cave) : base(cave.Row, cave.Column, "Cave", false)
        {
            _cave = cave;
        }

        CaveExitCell? Exit => _cave?.Exit;
        readonly CaveCell _cave;

        public override int Gold
        {
            get => Exit?.Gold ?? 0;
            set => Exit.Gold = value;
        }
        public override bool Galeon
        {
            get => Exit?.Galeon ?? false;
            set => Exit.Galeon = value;
        }

        public override void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            SelectableCoords.Add(Coords);
        }

        public override MovementResult AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            pirate.StartMove();
            return MovementResult.Continue;
        }
    }
}
