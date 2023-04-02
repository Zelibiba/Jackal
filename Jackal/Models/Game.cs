using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DynamicData.Binding;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;

namespace Jackal.Models
{
    /// <summary>
    /// Класс игровой модели.
    /// </summary>
    public static class Game
    {
        public static readonly int MapSize = 13;
        public static ObservableMap Map { get; private set; }
        public static Pirate? SelectedPirate { get; private set; }
        public static bool IsPirateSelected => SelectedPirate != null;

        public static void CreateMap()
        {
            Map = new ObservableMap(MapSize);
            for(int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                    Map.Add(new SeaCell(i, j));
            }
            for (int i = 1; i < MapSize - 1; i++)
            {
                for (int j = 1; j < MapSize - 1; j++)
                {
                    if ((i == 1 && j == 1) || (i == 1 && j == 11) || (i == 11 && j == 1) || (i == 11 && j == 11))
                        continue;
                    Map[i, j] = new Cell(i, j, "Field");
                }
            }
            Map[0, 6] = new ShipCell(0, 6, Team.White);
            foreach (Cell cell in Map)
                cell.SetSelectableCoords(Map);

            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[0, 6].AddPirate(new Pirate(Team.White));
        }
        
        public static void SelectPirate(Pirate pirate)
        {
            Deselect();
            SelectedPirate = pirate;

            foreach (Cell cell in Map)
                cell.CanBeSelected = false;
            foreach (int[] coords in pirate.Cell.SelectableCoords)
                Map[coords].CanBeSelected = true;
        }
        public static void Deselect(bool deselectPirate = true)
        {
            if (SelectedPirate == null)
                return;

            foreach (int[] coords in SelectedPirate.Cell.SelectableCoords)
                Map[coords].CanBeSelected = false;

            if (deselectPirate)
                SelectedPirate = null;
        }
        public static void MovePirate(Cell newCell)
        {
            Deselect(false);
            SelectedPirate.RemoveFromCell();
            newCell.AddPirate(SelectedPirate);

            //foreach (Cell cell in Map)
            //    cell.CanBeSelected = false;
            //foreach (int[] coords in newCell.SelectableCoords)
            //    Map[coords[0], coords[1]].CanBeSelected = true;
        }
    }
}
