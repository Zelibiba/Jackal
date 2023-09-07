using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    public class PitCell : Cell, ITrapCell
    {
        public PitCell(int row, int column) : base(row, column, "Pit")
        {
            AltSelectableCoords = new List<Coordinates>();
        }

        public List<Coordinates> AltSelectableCoords { get; }

        public override void SetSelectableCoords(Map map)
        {
            base.SetSelectableCoords(map);
            AltSelectableCoords.Clear();
            AltSelectableCoords.AddRange(SelectableCoords);
            SelectableCoords.Clear();
        }

        public override MovementResult AddPirate(Pirate pirate)
        {
            base.AddPirate(pirate);
            if (Pirates.Count == 1 && pirate is not Friday)
                pirate.IsBlocked = true;
            else if (Pirates.Count > 1)
            {
                Pirates[0].IsBlocked = false;
                foreach (Pirate p in Pirates)
                    p.IsDrunk = true;
            }
            return MovementResult.End;
        }
        public override void RemovePirate(Pirate pirate, bool withGold = true)
        {
            base.RemovePirate(pirate, withGold);
            pirate.IsBlocked = false;
        }
    }
}
