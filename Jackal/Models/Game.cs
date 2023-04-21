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

        public static ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();
        static Player CurrentPlayer => Players[CurrentPlayerNumber];
        static int CurrentPlayerNumber
        {
            get => __currentPlayerNumber;
            set
            {
                if (value >= Players.Count)
                    __currentPlayerNumber = value - Players.Count;
                else if (value < 0)
                    __currentPlayerNumber = Players.Count - value;
                else
                    __currentPlayerNumber = value;
            }
        }
        static int __currentPlayerNumber;
  
        public static Pirate? SelectedPirate
        {
            get => __selectedPirate;
            set
            {
                if (__selectedPirate != value)
                {
                    if (__selectedPirate != null)
                        __selectedPirate.IsSelected = false;

                    __selectedPirate = value;

                    if (__selectedPirate != null)
                        __selectedPirate.IsSelected = true;
                    else
                        DeselectInVM?.Invoke();
                }
            }
        }
        static Pirate? __selectedPirate;
        static bool IsPirateSelected => SelectedPirate != null;


        static ShipCell? SelectedShip { get; set; }
        static bool IsShipSelected => SelectedShip != null;


        static bool IsEarthQuake;
        static Cell? EarthQuakeSelectedCell;
        static bool PirateInMotion { get; set; }
        public static bool CanChangeSelection => !(PirateInMotion || IsEarthQuake);


        public static int LostGold { get; set; }

        public static void CreateMap()
        {
            #region создание каркаса
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
            #endregion

            Players.Add(new Player(0, "TEST", Team.White) { Turn = true });
            Players.Add(new Player(1, "AETHNAETRN", Team.Red));


            Map[0, 6] = new ShipCell(0, 6, Players[0], ShipRegions[0]);
            Map[1, 5] = new CannibalCell(1, 5);
            Map[1, 6] = new ArrowCell(1, 6, ArrowType.Side2, 1, ContinueMovePirate);
            Map[1, 7] = new FortressCell(1, 7, true);
            Map[2, 7] = new LakeCell(2, 7, ContinueMovePirate);
            Map[3, 8] = new LakeCell(3, 8, ContinueMovePirate);
            Map[4, 8] = new LakeCell(4, 8, ContinueMovePirate);
            Map[5, 8] = new LakeCell(5, 8, ContinueMovePirate);
            Map[2, 5] = new CrocodileCell(2, 5, ContinueMovePirate);
            Map[2, 6] = new EarthQuakeCell(2, 6);
            Map[5, 6] = new GunCell(5, 6, 1, ContinueMovePirate);
            foreach (Cell cell in Map)
                cell.SetSelectableCoords(Map);
        }

        static void NextPlayer()
        {
            CurrentPlayer.Turn = false;
            CurrentPlayerNumber++;
            CurrentPlayer.Turn = true;
        }

        public static Action<bool>? SetIsEnable;
        public static void PreSelectCell(Cell cell)
        {
            if (cell.CanBeSelected ||
                cell is ShipCell ship && ship.CanMove && !PirateInMotion)
                SelectCell(cell);
        }
        static void SelectCell(Cell cell)
        {
            Task.Run(() =>
            {
                SetIsEnable?.Invoke(false);
                if (IsPirateSelected && cell.CanBeSelected)
                {
                    Deselect(false);
                    StartMovePirate(cell.GetSelectedCell(SelectedPirate));
                }
                else if (cell is ShipCell)
                    SelectShip(cell);
                else if (IsShipSelected)
                    MoveShip(cell);
                else if (IsEarthQuake)
                    SelectEarthQuakeCell(cell);
                SetIsEnable?.Invoke(true);
            });
        }
        static void SelectShip(Cell cell)
        {
            Deselect();
            SelectedShip = cell as ShipCell;
            foreach (int[] coords in SelectedShip.MovableCoords)
                Map[coords].CanBeSelected = true;
        }
        static void SelectEarthQuakeCell(Cell cell)
        {
            if (cell.IsSelected)
            {
                cell.IsSelected = false;
                EarthQuakeSelectedCell = null;
            }
            else
            {
                if (EarthQuakeSelectedCell == null)
                {
                    cell.IsSelected = true;
                    EarthQuakeSelectedCell = cell;
                }
                else
                {
                    EarthQuakeSelectedCell.IsSelected = false;
                    SwapCells(EarthQuakeSelectedCell, cell);
                    Deselect();
                    EarthQuakeSelectedCell = null;
                    IsEarthQuake = false;
                    NextPlayer();
                }
            }
        }


        public static void SelectPirate(Pirate pirate, bool deselect = false)
        {
            Deselect(deselect);
            SelectedPirate = pirate;
            foreach (int[] coords in pirate.Cell.SelectableCoords)
            {
                Cell cell = Map[coords].GetSelectedCell(SelectedPirate);

                if (SelectedPirate.Treasure)
                    cell.CanBeSelected = cell.IsGoldFriendly();
                else
                    cell.CanBeSelected = true;
            }
        }
        public static void ReselctPirate(string param)
        {
            if (param == "gold")
                SelectedPirate.Gold = !SelectedPirate.Gold;
            else if (param == "galeon")
                SelectedPirate.Galeon = !SelectedPirate.Galeon;
            SelectPirate(SelectedPirate);
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
            else
            {
                foreach (Cell cell in Map)
                    cell.CanBeSelected = false;
            }
        }
        public static Action? DeselectInVM;


        public static Func<Cell,Task>? StartPirateAnimation;
        static void OnStartPirateAnimation(Cell cell)
        {
            if (StartPirateAnimation != null)
            {
                if (!cell.IsPreOpened)
                    cell.IsPreOpened = true;
                Dispatcher.UIThread.InvokeAsync(() => StartPirateAnimation(cell)).Wait();
                SelectedPirate.IsVisible = true;
            }
        }
        static void StartMovePirate(Cell cell)
        {
            SelectedPirate.TargetCell = cell;
            if (!PirateInMotion)
            {
                PirateInMotion = true;
                SelectedPirate.Set_StartCell();
            }

            OnStartPirateAnimation(cell);

            switch (MovePirate(cell))
            {
                case MovementResult.End:
                    SelectedPirate = null;
                    PirateInMotion = false;
                    NextPlayer();
                    break;
                case MovementResult.Continue:
                    SelectPirate(SelectedPirate); break;
                case MovementResult.EarthQuake:
                    SelectedPirate = null;
                    StartEarthQuake();
                    PirateInMotion = false;
                    break;
            }
        }
        static MovementResult ContinueMovePirate(int[] coords)
        {
            Cell cell = Map[coords].GetSelectedCell(SelectedPirate);
            
            if(!cell.IsGoldFriendly())
            {
                if (SelectedPirate.Gold)
                    SelectedPirate.Gold = false;
                if (SelectedPirate.Galeon)
                    SelectedPirate.Galeon = false;
            }

            OnStartPirateAnimation(cell);
            return MovePirate(cell);
        }
        static MovementResult MovePirate(Cell cell)
        {
            SelectedPirate.RemoveFromCell();
            return cell.AddPirate(SelectedPirate);
        }



        public static Func<Cell, Cell, Task>? StartCellAnimation;
        static void OnStartCellAnimation(Cell cell1, Cell cell2)
        {
            if (StartCellAnimation != null)
                Dispatcher.UIThread.InvokeAsync(() => StartCellAnimation(cell1, cell2)).Wait();
        }
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

            SelectedShip = null;
            NextPlayer();
        }
        static void SwapCells(Cell cell1, Cell cell2)
        {
            int[] coords1 = cell1.Coords;
            int[] coords2 = cell2.Coords;

            Cell cell = cell1;
            Map[coords1] = cell2;
            Map[coords1].SetCoordinates(coords1[0], coords1[1]);
            Map[coords1].SetSelectableCoords(Map);
            Map[coords2] = cell;
            Map[coords2].SetCoordinates(coords2[0], coords2[1]);
            Map[coords2].SetSelectableCoords(Map);

            OnStartCellAnimation(cell1, cell2);
        }

        static void StartEarthQuake()
        {
            IsEarthQuake = true;
            foreach (Cell cell in Map)
                cell.CanBeSelected = cell is not SeaCell && cell is not ShipCell &&
                                     cell.Gold == 0 && !cell.Galeon &&
                                     cell.Pirates.Count == 0;
        }

        public static void PirateBirth()
        {
            SelectedPirate.GiveBirth();
            Deselect();
            NextPlayer();
        }
    }
}
