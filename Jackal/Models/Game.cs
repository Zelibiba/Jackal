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
        static bool _falshStart = true;
        public static readonly int MapSize = 13;
        public static ObservableMap Map { get; private set; }
        public static Pirate? SelectedPirate { get; private set; }
        public static bool IsPirateSelected => SelectedPirate != null;
        public static ShipCell? SelectedShip { get;private set; }
        public static bool IsShipSelected => SelectedShip != null;

        public static bool CreateMap()
        {
            if(_falshStart)
            {
                _falshStart = false;
                return false;
            }

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

            (Orientation, int[][])[] ShipRegions = new (Orientation, int[][])[4];
            ShipRegions[0] = (Orientation.Down, new int[9][]);
            ShipRegions[1] = (Orientation.Left, new int[9][]);
            ShipRegions[2] = (Orientation.Up, new int[9][]);
            ShipRegions[3] = (Orientation.Right, new int[9][]);
            for (int i = 0; i < 9; i++)
            {
                ShipRegions[0].Item2[i] = new int[2] { 0, i + 2 };
                ShipRegions[1].Item2[i] = new int[2] { i + 2, 12 };
                ShipRegions[2].Item2[i] = new int[2] { 12, i + 2 };
                ShipRegions[3].Item2[i] = new int[2] { 12, i + 2 };
            }

            Map[0, 6] = new ShipCell(0, 6, Team.White, ShipRegions[0]);
            foreach (Cell cell in Map)
                cell.SetSelectableCoords(Map);

            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[1, 6].SetGold(2);

            return true;
        }
        
        public static void SelectCell(Cell cell)
        {
            if (IsPirateSelected)
                MovePirate(cell);
            else if (IsShipSelected)
                MoveShip(cell);
            else if (cell is ShipCell ship && ship.CanMove)
                SelectShip(ship);
        }
        public static void SelectPirate(Pirate pirate)
        {
            Deselect();
            SelectedPirate = pirate;
            foreach (int[] coords in pirate.Cell.SelectableCoords)
                Map[coords].CanBeSelected = true;
        }
        static void SelectShip(ShipCell ship)
        {
            Deselect();
            SelectedShip = ship;
            foreach (int[] coords in ship.MovableCoords)
                Map[coords].CanBeSelected = true;
        }
        public static void Deselect(bool deselect = true)
        {
            if (SelectedPirate != null)
            {
                foreach (int[] coords in SelectedPirate.Cell.SelectableCoords)
                    Map[coords].CanBeSelected = false;

                if (deselect)
                    SelectedPirate = null;
            }
            else if (SelectedShip != null)
            {
                foreach (int[] coords in SelectedShip.MovableCoords)
                    Map[coords].CanBeSelected = false;

                if (deselect)
                    SelectedShip = null;
            }
        }
        static void MovePirate(Cell newCell)
        {
            Deselect(false);
            SelectedPirate.RemoveFromCell();
            newCell.AddPirate(SelectedPirate);
            SelectedPirate = null;
        }
        static void MoveShip(Cell newCell)
        {
            Deselect(false);
            SwapCells(SelectedShip, newCell);

            if(SelectedShip.Orientation==Orientation.Up||SelectedShip.Orientation==Orientation.Down)
            {
                int row = SelectedShip.Row;
                row += (SelectedShip.Orientation == Orientation.Up) ? -1 : +1;
                int minColumn = Math.Min(SelectedShip.Column, newCell.Column) - 1;
                int maxColumn = Math.Max(SelectedShip.Column, newCell.Column) + 1;
                for (int j = minColumn; j <= maxColumn; j++)
                    Map[row, j].SetSelectableCoords(Map);
            }
            else
            {
                int column = SelectedShip.Column;
                column += (SelectedShip.Orientation == Orientation.Left) ? -1 : +1;
                int minRow = Math.Min(SelectedShip.Row, newCell.Row) - 1;
                int maxRow = Math.Max(SelectedShip.Row, newCell.Row) + 1;
                for (int i = minRow; i <= maxRow; i++)
                    Map[i, column].SetSelectableCoords(Map);
            }

            SelectedShip = null;
        }

        static void SwapCells(Cell cell1,Cell cell2)
        {
            int row1 = cell1.Row;
            int column1 = cell1.Column;
            int row2 = cell2.Row;
            int column2 = cell2.Column;

            Cell cell = cell1;
            Map[row1, column1] = cell2;
            Map[row1, column1].SetCoordinates(row1, column1);
            Map[row1, column1].SetSelectableCoords(Map);
            Map[row2, column2] = cell;
            Map[row2, column2].SetCoordinates(row2, column2);
            Map[row2, column2].SetSelectableCoords(Map);
        }
    }
}
