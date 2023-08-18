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

        public override void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if (i == 0 && j == 0)
                        continue;

                    int row = Row + i;
                    int column = Column + j;
                    if (map.CheckIndexes(row, column))
                        continue;
                    if (map[row, column] is not SeaCell && map[row, column] is not ShipCell)
                        continue;
                    SelectableCoords.Add(new int[] { row, column });
                }
            }
        }
    }
}
