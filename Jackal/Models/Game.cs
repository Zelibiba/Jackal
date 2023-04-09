using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData.Binding;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using ReactiveUI;

namespace Jackal.Models
{
    /// <summary>
    /// Класс игровой модели.
    /// </summary>
    public static class Game
    {
        public static readonly int MapSize = 13;
        public static ObservableMap Map { get; private set; }
  
        public static Pirate? SelectedPirate
        {
            get => _selectedPirate;
            set
            {
                if (_selectedPirate != value)
                {
                    if (_selectedPirate != null)
                        _selectedPirate.IsSelected = false;

                    _selectedPirate = value;

                    if (_selectedPirate != null)
                        _selectedPirate.IsSelected = true;
                    else
                        DeselectInVM?.Invoke();
                }
            }
        }
        static Pirate? _selectedPirate;
        public static bool IsPirateSelected => SelectedPirate != null;

        public static ShipCell? SelectedShip { get;private set; }
        public static bool IsShipSelected => SelectedShip != null;

        public static void Set_DeselectInVM(Action deselect) => DeselectInVM = deselect;
        public static void Set_StartPirateAnimation(Func<Cell, Task> function) => StartPirateAnimation = function;
        public static void Set_StartCellAnimation(Func<Cell,Cell,Task> function) => StartCellAnimation = function;
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
            Map[1, 7] = new HorseCell(1, 7);
            Map[3, 8] = new LakeCell(3, 8, ContinueMovePirate);
            Map[4, 8] = new LakeCell(4, 8, ContinueMovePirate);
            Map[5, 8] = new LakeCell(5, 8, ContinueMovePirate);
            foreach (Cell cell in Map)
                cell.SetSelectableCoords(Map);

            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[0, 6].AddPirate(new Pirate(Team.White));
            Map[1, 6].SetGold(2);

            Map[6, 6].AddPirate(new Pirate(Team.Red));
        }

        public static void PreSelectCell(Cell cell)
        {
            if (cell.CanBeSelected ||
                cell is ShipCell ship && ship.CanMove)
                SelectCell(cell);
        }
        static void SelectCell(Cell cell)
        {
            Task.Run(() =>
            {
                if (IsPirateSelected && cell.CanBeSelected)
                {
                    Deselect(false);
                    OnStartPirateAnimation(cell);
                }
                else if (cell is ShipCell)
                    SelectShip(cell);
                else if (IsShipSelected)
                    MoveShip(cell);
            });
        }
        static void SelectShip(Cell cell)
        {
            Deselect();
            SelectedShip = cell as ShipCell;
            foreach (int[] coords in SelectedShip.MovableCoords)
                Map[coords].CanBeSelected = true;
        }

        public static bool PreSelectPirate(Pirate pirate)
        {
            SelectPirate(pirate);
            return true;
        }
        static void SelectPirate(Pirate pirate, bool deselect = true)
        {
            Deselect(deselect);
            SelectedPirate = pirate;
            foreach (int[] coords in pirate.Cell.SelectableCoords)
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
        static Action? DeselectInVM;


        static Func<Cell,Task>? StartPirateAnimation;
        static void OnStartPirateAnimation(Cell cell)
        {
            if (StartPirateAnimation != null)
                Dispatcher.UIThread.InvokeAsync(() => StartPirateAnimation(cell)).Wait();
            SelectedPirate.IsVisible = true;

            if (MovePirate(cell))
                SelectedPirate = null;
            else
                SelectPirate(SelectedPirate, false);
        }
        static bool ContinueMovePirate(int[] coords)
        {
            Cell cell = Map[coords];
            if (StartPirateAnimation != null)
                Dispatcher.UIThread.InvokeAsync(() => StartPirateAnimation(cell)).Wait();
            SelectedPirate.IsVisible = true;

            return MovePirate(cell);
        }
        static bool MovePirate(Cell newCell)
        {
            SelectedPirate.RemoveFromCell();
            return newCell.AddPirate(SelectedPirate);
        }


        static Func<Cell, Cell, Task>? StartCellAnimation;
        static void MoveShip(Cell newCell)
        {
            Deselect(false);
            SwapCells(SelectedShip, newCell);

            if (SelectedShip.Orientation == Orientation.Up || SelectedShip.Orientation == Orientation.Down)
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

            if (StartCellAnimation != null)
                Dispatcher.UIThread.InvokeAsync(() => StartCellAnimation(SelectedShip, newCell)).Wait();

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
