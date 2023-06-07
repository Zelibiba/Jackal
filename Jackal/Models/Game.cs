using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
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
        /// <summary>
        /// Размер карты.
        /// </summary>
        public static readonly int MapSize = 13;
        /// <summary>
        /// Карта.
        /// </summary>
        /// <remarks>
        /// Представляяет собой массив клеток типа <see cref="Cell"/>.
        /// </remarks>
        public static ObservableMap Map { get; private set; }

        /// <summary>
        /// Лист игроков.
        /// </summary>
        public static ObservableCollection<Player> Players { get; } = new ObservableCollection<Player>();
        /// <summary>
        /// Игрок, активный в текущий момент.
        /// </summary>
        static Player CurrentPlayer => Players[CurrentPlayerNumber];
        /// <summary>
        /// Номер текущего игрока в списке.
        /// </summary>
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

        /// <summary>
        /// Пират, выбранный на текущий момент.
        /// </summary>
        /// <remarks>
        /// Содержит логику.
        /// </remarks>
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
                        DeselectPirate?.Invoke();
                }
            }
        }
        static Pirate? __selectedPirate;
        /// <summary>
        /// Флаг того, что какой-то пират выбран.
        /// </summary>
        static bool IsPirateSelected => SelectedPirate != null;

        /// <summary>
        /// Корабль, выбранный на текущий момент.
        /// </summary>
        static ShipCell? SelectedShip { get; set; }
        /// <summary>
        /// Флаг того, что какой-то корабль выбран.
        /// </summary>
        static bool IsShipSelected => SelectedShip != null;

        /// <summary>
        /// Флаг того, что происходит землетрясение.
        /// </summary>
        static bool IsEarthQuake;
        /// <summary>
        /// Клетка, выбранная в течение землетрясения.
        /// </summary>
        static Cell? EarthQuakeSelectedCell;
        /// <summary>
        /// Флаг того, что некоторый пират начал ходить.
        /// </summary>
        /// <remarks>
        /// Необходим для блокировки выбора других пиратов и корабля.
        /// </remarks>
        static bool PirateInMotion { get; set; }
        /// <summary>
        /// Флаг того, что пирата напоили ромом.
        /// </summary>
        static bool PirateIsDrunk { get; set; }
        /// <summary>
        /// Флаг того, что можно выбрать другого пирата или снять выделение.
        /// </summary>
        public static bool CanChangeSelection => !(PirateInMotion || PirateIsDrunk || IsEarthQuake);

        /// <summary>
        /// Счётчик потерянного золота.
        /// </summary>
        public static int LostGold { get; set; }

        /// <summary>
        /// Метод инициализирует игру.
        /// </summary>
        public static void CreateMap()
        {
            #region создание каркаса карты
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
                ShipRegions[3].Item2[i] = new int[2] { i + 2, 0 };
            }
            #endregion

            Players.Add(new Player(0, "TEST", Team.White) { Turn = true });
            Players.Add(new Player(1, "AETHNAETRN", Team.Red));


            Map[0, 6] = new ShipCell(0, 6, Players[0], ShipRegions[0]);
            Map[12, 6] = new ShipCell(12, 6, Players[1], ShipRegions[2]);
            Map[2, 7] = new JungleCell(2, 7);
            foreach (Pirate pirate in Players[1].Pirates)
            {
                pirate.RemoveFromCell();
                Map[2, 6].AddPirate(pirate);
            }
            foreach (Cell cell in Map)
                cell.SetSelectableCoords(Map);
        }




        /// <summary>
        /// Метод передаёт ход следующему игроку.
        /// </summary>
        static void NextPlayer()
        {
            CurrentPlayer.Turn = false;
            CurrentPlayerNumber++;
            CurrentPlayer.Turn = true;
        }

        /// <summary>
        /// Делегат для блокировки интерфейса.
        /// </summary>
        public static Action<bool>? SetIsEnable;
        /// <summary>
        /// Метод проверки возможности выбора клетки.
        /// </summary>
        /// <param name="cell">Выбираемая клетка.</param>
        public static void PreSelectCell(Cell cell)
        {
            if (cell.CanBeSelected ||
                cell is ShipCell ship && ship.CanMove && CanChangeSelection)
                SelectCell(cell);
        }
        /// <summary>
        /// Метод выбора клетки.
        /// </summary>
        /// <param name="cell">Выбираемая клетка.</param>
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
        /// <summary>
        /// Метод выбора корабля.
        /// </summary>
        /// <param name="cell">Выбираемый корабль.</param>
        static void SelectShip(Cell cell)
        {
            Deselect();
            SelectedShip = cell as ShipCell;
            foreach (int[] coords in SelectedShip.MovableCoords)
                Map[coords].CanBeSelected = true;
        }
        /// <summary>
        /// Метод выбора клетки во время землетрясения.
        /// </summary>
        /// <param name="cell">Выбираемая клетка.</param>
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


        /// <summary>
        /// Метод выбора пирата.
        /// </summary>
        /// <param name="pirate">Выбираемый пират.</param>
        public static void SelectPirate(Pirate pirate)
        {
            Deselect(false);
            SelectedPirate = pirate;
            foreach (int[] coords in pirate.SelectableCoords)
            {
                Cell cell = Map[coords].GetSelectedCell(pirate);
                cell.DefineSelectability(pirate);
            }
        }
        /// <summary>
        /// Метод перевыделения пирата при изменении параметров перетаскивания сокровища.
        /// </summary>
        /// <param name="param">Тип перетаскиваемого сокровища.</param>
        public static void ReselctPirate(string param)
        {
            if (param == "gold")
                SelectedPirate.Gold = !SelectedPirate.Gold;
            else if (param == "galeon")
                SelectedPirate.Galeon = !SelectedPirate.Galeon;
            SelectPirate(SelectedPirate);
        }
        /// <summary>
        /// Метод отмены возможности выделения с клеток.
        /// </summary>
        /// <param name="deselect">Флаг того, что необходимо отменить выделения текущего пирата или корабля.</param>
        public static void Deselect(bool deselect = true)
        {
            if (SelectedPirate != null)
            {
                foreach (int[] coords in SelectedPirate.SelectableCoords)
                    Map[coords].GetSelectedCell(SelectedPirate).CanBeSelected = false;

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
        /// <summary>
        /// Делегат для отмены выделения текущего пирата в интерфейсе.
        /// </summary>
        public static Action? DeselectPirate;


        /// <summary>
        /// Делегат запуска анимации перемещения пирата.
        /// </summary>
        public static Func<Cell,Task>? StartPirateAnimation;
        /// <summary>
        /// Метод запуска анимации перемещения пирата.
        /// </summary>
        /// <param name="cell">Клетка, куда перемещается пират.</param>
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
        /// <summary>
        /// Метод обработки перемещения пирата.
        /// </summary>
        /// <param name="cell">Клетка, куда перемещается пират.</param>
        static void StartMovePirate(Cell cell)
        {
            SelectedPirate.TargetCell = cell;
            if (!PirateInMotion)
            {
                PirateInMotion = true;
                SelectedPirate.Set_StartCell();
                if (PirateIsDrunk)
                    PirateIsDrunk = false;
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
        /// <summary>
        /// Метод безусловного продолжения перемещения пирата.
        /// </summary>
        /// <param name="coords">Координаты клетки, куда перемещается пират.</param>
        /// <returns></returns>
        static MovementResult ContinueMovePirate(int[] coords)
        {
            Cell cell = Map[coords].GetSelectedCell(SelectedPirate);
            
            if(!cell.IsGoldFriendly(SelectedPirate))
            {
                if (SelectedPirate.Gold)
                    SelectedPirate.Gold = false;
                if (SelectedPirate.Galeon)
                    SelectedPirate.Galeon = false;
            }

            OnStartPirateAnimation(cell);
            return MovePirate(cell);
        }
        /// <summary>
        /// Метод перемещения пирата.
        /// </summary>
        /// <param name="cell">Клетка, куда перемещается пират.</param>
        /// <returns>Флаг действия, которое необходимо выполнить.</returns>
        static MovementResult MovePirate(Cell cell)
        {
            SelectedPirate.RemoveFromCell();
            return cell.AddPirate(SelectedPirate);
        }


        /// <summary>
        /// Делегат запуска анимации премещения клеток.
        /// </summary>
        public static Func<Cell, Cell, Task>? StartCellAnimation;
        /// <summary>
        /// Метод запуска анимации премещения клеток.
        /// </summary>
        /// <param name="cell1">Первая перемещаемая клетка.</param>
        /// <param name="cell2">Вторая перемещаемая клетка.</param>
        static void OnStartCellAnimation(Cell cell1, Cell cell2)
        {
            if (StartCellAnimation != null)
                Dispatcher.UIThread.InvokeAsync(() => StartCellAnimation(cell1, cell2)).Wait();
        }
        /// <summary>
        /// Метод перемещения корабля.
        /// </summary>
        /// <param name="newCell">Клетка, на которую перемещается корабль.</param>
        static void MoveShip(Cell cell)
        {
            Deselect(false);
            SwapCells(SelectedShip, cell);

            // Подобрать или убить пиратов в море
            if (cell.Pirates.Count > 0)
            {
                if (SelectedShip.IsFriendlyTo(cell.Pirates[0]))
                {
                    SelectedShip.Pirates.AddRange(cell.Pirates);
                    foreach (Pirate pirate in cell.Pirates)
                        pirate.Cell = SelectedShip;
                    cell.Pirates.Clear();
                }
                else
                {
                    while(cell.Pirates.Count > 0)
                        cell.Pirates[0].Kill();
                }
            }

            // обновить достигаемые координаты для побережья
            if (SelectedShip.Orientation == Orientation.Up || SelectedShip.Orientation == Orientation.Down)
            {
                int row = SelectedShip.Row;
                row += (SelectedShip.Orientation == Orientation.Up) ? -1 : +1;
                int minColumn = Math.Min(SelectedShip.Column, cell.Column) - 1;
                int maxColumn = Math.Max(SelectedShip.Column, cell.Column) + 1;
                for (int j = minColumn; j <= maxColumn; j++)
                    Map[row, j].SetSelectableCoords(Map);
            }
            else
            {
                int column = SelectedShip.Column;
                column += (SelectedShip.Orientation == Orientation.Left) ? -1 : +1;
                int minRow = Math.Min(SelectedShip.Row, cell.Row) - 1;
                int maxRow = Math.Max(SelectedShip.Row, cell.Row) + 1;
                for (int i = minRow; i <= maxRow; i++)
                    Map[i, column].SetSelectableCoords(Map);
            }

            SelectedShip = null;
            NextPlayer();
        }
        /// <summary>
        /// Метод для перемены двух клеток местами.
        /// </summary>
        /// <param name="cell1">Первая перемещаемая клетка.</param>
        /// <param name="cell2">Вторая перемещаемая клетка.</param>
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

        /// <summary>
        /// Метод запуска землетрясения.
        /// </summary>
        static void StartEarthQuake()
        {
            IsEarthQuake = true;
            foreach (Cell cell in Map)
                cell.CanBeSelected = cell is not SeaCell && cell is not ShipCell &&
                                     cell.Gold == 0 && !cell.Galeon &&
                                     cell.Pirates.Count == 0;
        }

        /// <summary>
        /// Метод спаивания выбранного пирата.
        /// </summary>
        public static void GetPirateDrunk()
        {
            PirateIsDrunk = true;
            CurrentPlayer.Bottles--;
            Deselect(false);
            SelectedPirate.IsDrunk = true;
            SelectPirate(SelectedPirate);
        }

        /// <summary>
        /// Метод рождения пирата.
        /// </summary>
        public static void PirateBirth()
        {
            SelectedPirate.GiveBirth();
            Deselect();
            NextPlayer();
        }
    }
}
