﻿using System;
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
            IsOpened = true;
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

                    int newI = Row + i;
                    int newJ = Column + j;
                    if (newI < 0 || newJ < 0 || newI >= map.MapSize || newJ >= map.MapSize)
                        continue;
                    if (map[newI, newJ] is not SeaCell && map[newI, newJ] is not ShipCell)
                        continue;
                    SelectableCoords.Add(new int[] { newI, newJ });
                }
            }
        }
    }
}
