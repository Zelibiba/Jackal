﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using Jackal.Network;
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
        public static Player CurrentPlayer => Players[CurrentPlayerNumber];
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
            private set
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
        public static bool CanChangeSelection => !(PirateInMotion || PirateIsDrunk || earthQuake.IsActive || lightHouse.IsActive);

        /// <summary>
        /// Счётчик спрятанного золота.
        /// </summary>
        /// <remarks>Имеет логику, связанную с <see cref="CurrentGold"/>.</remarks>
        public static int HiddenGold
        {
            get => __hiddenGold;
            set
            {
                CurrentGold += __hiddenGold - value;
                __hiddenGold = value;
            }
        }
        static int __hiddenGold = 40;
        /// <summary>
        /// Счётчик видимого золота на карте.
        /// </summary>
        public static int CurrentGold { get; set; }
        /// <summary>
        /// Счётчик потерянного золота.
        /// </summary>
        /// <remarks>Имеет логику, связанную с <see cref="CurrentGold"/>.</remarks>
        public static int LostGold
        {
            get => __lostGold;
            set
            {
                CurrentGold += __lostGold - value;
                __lostGold = value;
            }
        }
        static int __lostGold;

        /// <summary>
        /// Метод инициализирует игру.
        /// </summary>
        /// <param name="players">Упорядоченный список игроков.</param>
        /// <param name="seed">Сид для генерации карты.</param>
        /// <param name="autosave">Флаг того, что необходимо включить автосохранения.</param>
        public static void CreateMap(IEnumerable<Player> players, int seed = -1, bool autosave = true)
        {
            #region создание паттерна клеток
            List<string> pattern = new List<string>(117);
            for (int i = 0; i < 18; i++)
                pattern.Add("field");
            for (int i = 0; i < 2; i++)
                pattern.Add("horse");
            for (int i = 0; i < 4; i++)
                pattern.Add("rum");
            for (int i = 0; i < 6; i++)
                pattern.Add("lake");
            for (int i = 0; i < 3; i++)
                pattern.Add("pit");
            for (int i = 0; i < 4; i++)
                pattern.Add("crocodile");
            for (int i = 0; i < 2; i++)
                pattern.Add("fortress");
            for (int i = 0; i < 2; i++)
                pattern.Add("balloon");
            for (int i = 0; i < 4; i++)
                pattern.Add("cave");
            for (int i = 0; i < 3; i++)
                pattern.Add("jungle");
            for (int i = 0; i < 2; i++)
                pattern.Add("cannabis");
            for (int i = 0; i < 2; i++)
                pattern.Add("gun");

            for (int i = 0; i < 3; i++)
                pattern.Add("arrow_1a");
            for (int i = 0; i < 3; i++)
                pattern.Add("arrow_1s");
            for (int i = 0; i < 3; i++)
                pattern.Add("arrow_2a");
            for (int i = 0; i < 3; i++)
                pattern.Add("arrow_2s");
            for (int i = 0; i < 3; i++)
                pattern.Add("arrow_3");
            for (int i = 0; i < 3; i++)
                pattern.Add("arrow_4a");
            for (int i = 0; i < 3; i++)
                pattern.Add("arrow_4s");

            for (int i = 0; i < 5; i++)
                pattern.Add("maze_2");
            for (int i = 0; i < 4; i++)
                pattern.Add("maze_3");
            for (int i = 0; i < 2; i++)
                pattern.Add("maze_4");
            pattern.Add("maze_5");

            for (int i = 0; i < 5; i++)
                pattern.Add("gold_1");
            for (int i = 0; i < 5; i++)
                pattern.Add("gold_2");
            for (int i = 0; i < 3; i++)
                pattern.Add("gold_3");
            for (int i = 0; i < 2; i++)
                pattern.Add("gold_4");
            pattern.Add("gold_5");
            pattern.Add("galeon");

            for (int i = 0; i < 3; i++)
                pattern.Add("bottle_1");
            for (int i = 0; i < 2; i++)
                pattern.Add("bottle_2");
            pattern.Add("bottle_3");

            pattern.Add("cannibal");
            pattern.Add("putana");
            pattern.Add("airplane");
            pattern.Add("friday");
            pattern.Add("missioner");
            pattern.Add("ben");
            pattern.Add("earthquake");
            pattern.Add("carramba");
            pattern.Add("lighthouse");
            #endregion

            #region создание моря и регионов кораблей
            Map = new ObservableMap(MapSize);
            for (int i = 0; i < MapSize; i++)
            {
                for (int j = 0; j < MapSize; j++)
                    Map.Add(new SeaCell(i, j));
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

            #region создание карты
            Random rand;
            if (seed == -1)
            {
                rand = new Random();
                seed = rand.Next();
            }
            rand = new Random(seed);

            List<CaveCell> caveCells = new(4);
            for (int i = 1; i < MapSize - 1; i++)
            {
                for (int j = 1; j < MapSize - 1; j++)
                {
                    if ((i == 1 && j == 1) || (i == 1 && j == 11) || (i == 11 && j == 1) || (i == 11 && j == 11))
                        continue;

                    int index = rand.Next(pattern.Count);
                    string name = pattern[index];
                    pattern.RemoveAt(index);
                    switch (name.Split('_')[0])
                    {
                        case "field":
                            Map[i, j] = new Cell(i, j, "Field");
                            break;
                        case "horse":
                            Map[i, j] = new HorseCell(i, j);
                            break;
                        case "rum":
                            Map[i, j] = new RumCell(i, j);
                            break;
                        case "lake":
                            Map[i, j] = new LakeCell(i, j, ContinueMovePirate);
                            break;
                        case "pit":
                            Map[i, j] = new PitCell(i, j);
                            break;
                        case "crocodile":
                            Map[i, j] = new CrocodileCell(i, j, ContinueMovePirate);
                            break;
                        case "fortress":
                            Map[i, j] = new FortressCell(i, j, false);
                            break;
                        case "balloon":
                            Map[i, j] = new BalloonCell(i, j, ContinueMovePirate);
                            break;
                        case "cave":
                            Map[i, j] = new CaveCell(i, j, ContinueMovePirate);
                            caveCells.Add(Map[i, j] as CaveCell);
                            break;
                        case "jungle":
                            Map[i, j] = new JungleCell(i, j);
                            break;
                        case "cannabis":
                            Map[i, j] = new CannabisCell(i, j);
                            break;
                        case "gun":
                            int rotation = rand.Next(4);
                            Map[i, j] = new GunCell(i, j, rotation, ContinueMovePirate);
                            break;
                        case "arrow":
                            rotation = rand.Next(4);
                            ArrowType arrowType;
                            switch (name.Split('_')[1])
                            {
                                case "1a":
                                    arrowType = ArrowType.Angle1;
                                    break;
                                case "1s":
                                    arrowType = ArrowType.Side1;
                                    break;
                                case "2a":
                                    arrowType = ArrowType.Angle2;
                                    break;
                                case "2s":
                                    arrowType = ArrowType.Side2;
                                    break;
                                case "3":
                                    arrowType = ArrowType.Angle3;
                                    break;
                                case "4a":
                                    arrowType = ArrowType.Angle4;
                                    break;
                                case "4s":
                                    arrowType = ArrowType.Side4;
                                    break;
                                default:
                                    throw new Exception("Wrong random ArrowType");
                            }
                            Map[i, j] = new ArrowCell(i, j, arrowType, rotation, ContinueMovePirate);
                            break;
                        case "maze":
                            int size = int.Parse(name.Split('_')[1]);
                            Map[i, j] = new MazeCell(i, j, size);
                            break;
                        case "gold":
                            GoldType gold;
                            switch (name.Split('_')[1])
                            {
                                case "1":
                                    gold = GoldType.Gold1;
                                    break;
                                case "2":
                                    gold = GoldType.Gold2;
                                    break;
                                case "3":
                                    gold = GoldType.Gold3;
                                    break;
                                case "4":
                                    gold = GoldType.Gold4;
                                    break;
                                case "5":
                                    gold = GoldType.Gold5;
                                    break;
                                default:
                                    throw new Exception("Wrong random Gold Type");
                            }
                            Map[i, j] = new GoldCell(i, j, gold);
                            break;
                        case "galeon":
                            Map[i, j] = new GoldCell(i, j, GoldType.Galeon);
                            break;
                        case "bottle":
                            int count = int.Parse(name.Split('_')[1]);
                            Map[i, j] = new BottleCell(i, j, count);
                            break;
                        case "cannibal":
                            Map[i, j] = new CannibalCell(i, j);
                            break;
                        case "putana":
                            Map[i, j] = new FortressCell(i, j, true);
                            break;
                        case "airplane":
                            Map[i, j] = new AirplaneCell(i, j);
                            break;
                        case "friday":
                            Map[i, j] = new ResidentCell(i, j, ResidentType.Friday);
                            break;
                        case "missioner":
                            Map[i, j] = new ResidentCell(i, j, ResidentType.Missioner);
                            break;
                        case "ben":
                            Map[i, j] = new ResidentCell(i, j, ResidentType.Ben);
                            break;
                        case "earthquake":
                            Map[i, j] = new EarthQuakeCell(i, j);
                            break;
                        case "carramba":
                            Map[i, j] = new Cell(i, j, "Field");
                            break;
                        case "lighthouse":
                            Map[i, j] = new LightHouseCell(i, j);
                            break;
                        default:
                            throw new Exception("Wrong random cell");
                    }
                }
            }
            foreach (CaveCell cave in caveCells)
                cave.LinkCaves(caveCells);
            #endregion

            foreach (Player player in players)
                Players.Add(player);

            Map[0, 6] = new ShipCell(0, 6, Players[0], ShipRegions[0]);
            switch (Players.Count)
            {
                case 2:
                    Map[12, 6] = new ShipCell(12, 6, Players[1], ShipRegions[2]); break;
                case 3:
                    Map[6, 12] = new ShipCell(6, 12, Players[1], ShipRegions[1]);
                    Map[6, 0] = new ShipCell(6, 0, Players[2], ShipRegions[3]);
                    break;
                case 4:
                    Map[6, 12] = new ShipCell(6, 12, Players[1], ShipRegions[1]);
                    Map[12, 6] = new ShipCell(12, 6, Players[2], ShipRegions[2]);
                    Map[6, 0] = new ShipCell(6, 0, Players[3], ShipRegions[3]);
                    break;
            }
            foreach (Cell cell in Map)
                cell.SetSelectableCoords(Map);

            if (autosave)
                FileHandler.StartAutosave(Players, seed);

            CurrentPlayerNumber = Players.Count - 1;
            NextPlayer();
        }




        /// <summary>
        /// Метод передаёт ход следующему игроку.
        /// </summary>
        static void NextPlayer(bool checkOnly = false)
        {
            #region Обработка бочки рома
            foreach (Pirate pirate in CurrentPlayer.Pirates)
            {
                if (pirate.RumCount == 1)
                {
                    pirate.IsEnabled = true;
                    pirate.IsBlocked = false;
                }
                if (pirate.RumCount > 0)
                    pirate.RumCount--;
            }
            #endregion

            if (!checkOnly)
            {
                if (CurrentPlayer.CannabisStarter)
                    EndCannabis();
                else
                {
                    CurrentPlayer.Turn = false;
                    CurrentPlayerNumber++;
                    CurrentPlayer.Turn = true;

                    // проверка на возможность споить миссионера или пятницу
                    foreach (Pirate pirate in CurrentPlayer.Pirates)
                        pirate.DefineDrinkingOpportynities(Map);
                }
            }
        }

        public static Action<bool>? EnableInterface;
        /// <summary>
        /// Метод проверки возможности выбора клетки.
        /// </summary>
        /// <param name="cell">Выбираемая клетка.</param>
        public static bool PreSelectCell(Cell cell)
        {
            return cell.CanBeSelected ||
                   cell is ShipCell ship && ship.CanMove && ship.Manager == CurrentPlayer && CanChangeSelection;
        }
        /// <summary>
        /// Метод выбора клетки.
        /// </summary>
        /// <param name="cell">Выбираемая клетка.</param>
        public static void SelectCell(Cell cell)
        {
            if (CurrentPlayer.IsControllable)
                EnableInterface?.Invoke(false);

            if (IsPirateSelected && cell.CanBeSelected)
            {
                Deselect(false);
                StartMovePirate(cell.GetSelectedCell(SelectedPirate));
            }
            else if (cell is ShipCell)
                SelectShip(cell);
            else if (IsShipSelected)
                MoveShip(cell);
            else if (earthQuake.IsActive)
                SelectEarthQuakeCell(cell);
            else if (lightHouse.IsActive)
                SelectLightHouseCell(cell);

            if (CurrentPlayer.IsControllable)
                EnableInterface?.Invoke(true);
        }
        /// <summary>
        /// <inheritdoc cref="SelectCell(Cell)" path="/summary"/>
        /// </summary>
        /// <remarks>Необходима для <see cref="FileHandler.ReadSave(string)"/> и <see cref="Client"/>.</remarks>
        /// <param name="coordinates">Координаты выбранной ячейки.</param>
        public static void SelectCell(int[] coordinates) => SelectCell(Map[coordinates]);
        /// <summary>
        /// Метод выбора корабля.
        /// </summary>
        /// <param name="cell">Выбираемый корабль.</param>
        static void SelectShip(Cell cell)
        {
            Deselect();
            SelectedShip = cell as ShipCell;
            foreach (Cell _cell in Map.Cells(SelectedShip.MovableCoords))
                _cell.CanBeSelected = true;
        }
        /// <summary>
        /// Метод выбора клетки во время землетрясения.
        /// </summary>
        /// <param name="cell">Выбираемая клетка.</param>
        static void SelectEarthQuakeCell(Cell cell)
        {
            FileHandler.EarthQuake(cell);

            if (earthQuake.SelectedCell == null)
                earthQuake.SelectCell(cell);
            else if (cell.IsSelected)
                earthQuake.DeselectCell();
            else
            {
                Deselect();
                SwapCells(earthQuake.SelectedCell, cell);
                earthQuake.DeselectCell();
                earthQuake.IsActive = false;
                NextPlayer();
            }
        }
        /// <summary>
        /// Метод выбора клетки во время хода маяка.
        /// </summary>
        /// <param name="cell">Выбираемая клетка.</param>
        static void SelectLightHouseCell(Cell cell)
        {
            FileHandler.LightHouse(cell);

            if (lightHouse.SelectedCells.Count < lightHouse.SelectedCells.Capacity)
            {
                cell.CanBeSelected = false;
                cell.IsSelected = true;
                cell.IsLightHousePicked = true;
                cell.IsPreOpened = true;
                lightHouse.SelectedCells.Add(cell);
                if (lightHouse.SelectedCells.Count == lightHouse.SelectedCells.Capacity)
                {
                    Deselect();
                    foreach (Cell c in lightHouse.SelectedCells)
                    {
                        c.CanBeSelected = true;
                        c.IsSelected = false;
                    }
                    lightHouse.Lighthouse.CanBeSelected = true;
                }
            }
            else
            {
                if (cell is LightHouseCell)
                {
                    cell.CanBeSelected = false;
                    foreach (Cell c in lightHouse.SelectedCells)
                    {
                        c.CanBeSelected = false;
                        c.IsSelected = false;
                        c.ChangeGrayStatus();
                    }
                    lightHouse.IsActive = false;
                    NextPlayer();
                }
                else if (lightHouse.SelectedCell == null)
                    lightHouse.SelectCell(cell);
                else if (cell.IsSelected)
                    lightHouse.DeselectCell();
                else
                {
                    SwapCells(cell, lightHouse.SelectedCell);
                    lightHouse.DeselectCell();
                }
            }
        }

        /// <summary>
        /// Метод проверки возможности выбора пирата.
        /// </summary>
        /// <param name="pirate"></param>
        /// <returns></returns>
        public static bool PreSelectPirate(Pirate pirate) => pirate.Manager == CurrentPlayer;
        /// <summary>
        /// Метод выбора пирата.
        /// </summary>
        /// <param name="pirate">Выбираемый пират.</param>
        public static void SelectPirate(Pirate pirate)
        {
            Deselect(false);
            SelectedPirate = pirate;
            foreach (Cell cell in Map.Cells(pirate))
                cell.GetSelectedCell(pirate).DefineSelectability(pirate);
        }
        /// <summary>
        /// Метод тихой выборки пирата без обработки интерфейса и достижимых координат.
        /// </summary>
        /// <remarks>Необходима для <see cref="FileHandler.ReadSave(string)"/> и <see cref="Client"/>.</remarks>
        /// <param name="pirateIndex">Индекс пирата в списке пиратов у игрока.</param>
        /// <param name="gold">Флаг того, что пират понесёт золото.</param>
        /// <param name="galeon">Флаг того, что пират понесёт Галеон.</param>
        public static void SelectPirate(int pirateIndex, bool gold = false ,bool galeon = false)
        {
            SelectPirate(CurrentPlayer.Pirates[pirateIndex]);
            SelectedPirate.Gold = gold;
            SelectedPirate.Galeon = galeon;
        }
        /// <summary>
        /// Метод перевыделения пирата при изменении параметров перетаскивания сокровища.
        /// </summary>
        /// <param name="param">Тип перетаскиваемого сокровища.</param>
        public static void GrabGold(string param)
        {
            if (param == "gold")
            {
                SelectedPirate.Gold = !SelectedPirate.Gold;
                SelectedPirate.Galeon = false;
            }
            else if (param == "galeon")
            {
                SelectedPirate.Galeon = !SelectedPirate.Galeon;
                SelectedPirate.Gold = false;
            }
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
                foreach (Cell cell in Map.Cells(SelectedPirate))
                    cell.GetSelectedCell(SelectedPirate).CanBeSelected = false;

                if (deselect)
                    SelectedPirate = null;
            }
            else if (SelectedShip != null)
            {
                foreach (Cell cell in Map.Cells(SelectedShip.MovableCoords))
                    cell.CanBeSelected = false;

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
            }
        }
        /// <summary>
        /// Метод обработки перемещения пирата.
        /// </summary>
        /// <param name="cell">Клетка, куда перемещается пират.</param>
        static void StartMovePirate(Cell cell)
        {
            FileHandler.MovePirate(SelectedPirate, cell);
            Client.MovePirate(SelectedPirate, cell);

            SelectedPirate.TargetCell = cell;
            if (!PirateInMotion)
            {
                PirateInMotion = true;
                SelectedPirate.SetStartCell();
                if (PirateIsDrunk)
                    PirateIsDrunk = false;
            }

            OnStartPirateAnimation(cell);

            MovementResult result = MovePirate(cell);
            SelectedPirate.IsVisible = true;
            switch (result)
            {
                case MovementResult.End:
                    if (SelectedPirate is Friday && cell.ContainsMissioner || SelectedPirate is Missioner && cell.ContainsFriday)
                    {
                        SelectedPirate.Kill();
                        cell.Pirates.First(pirate => pirate is Friday || pirate is Missioner).Kill();
                    }
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
                case MovementResult.LightHouse:
                    LightHouseCell lightHouse = SelectedPirate.Cell as LightHouseCell;
                    SelectedPirate = null;
                    StartLightHouse(lightHouse);
                    PirateInMotion = false;
                    break;
                case MovementResult.Cannabis:
                    SelectedPirate = null;
                    PirateInMotion = false;
                    StartCannabis();
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

            OnStartPirateAnimation(cell);

            if (!cell.IsGoldFriendly(SelectedPirate))
            {
                if (SelectedPirate.Gold)
                    SelectedPirate.Gold = false;
                if (SelectedPirate.Galeon)
                    SelectedPirate.Galeon = false;
            }
            if(!cell.CanBeSelectedBy(SelectedPirate))
            {
                SelectedPirate.Kill();
                return MovementResult.End;
            }

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
            FileHandler.MoveShip(cell);

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
        /// <param name="animation">Флаг того, что перемещение ячеек нужно анимировать.</param>
        static void SwapCells(Cell cell1, Cell cell2, bool animation = true)
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

            if (animation)
                OnStartCellAnimation(cell1, cell2);
        }

        /// <summary>
        /// Переменная для управления землетрясением.
        /// </summary>
        static readonly EarthQuake earthQuake = new();
        /// <summary>
        /// Переменная для управления ходом маяка.
        /// </summary>
        static readonly LightHouse lightHouse = new();
        /// <summary>
        /// Метод запуска землетрясения.
        /// </summary>
        static void StartEarthQuake()
        {
            earthQuake.IsActive = true;
            foreach (Cell cell in Map)
                cell.CanBeSelected = cell is not SeaCell && cell is not ShipCell &&
                                     cell.Gold == 0 && !cell.Galeon &&
                                     cell.Pirates.Count == 0;
        }
        /// <summary>
        /// Метод запуска хода маяка.
        /// </summary>
        /// <param name="LightHouse">Клетка маяка, вызвавшая этот ход маяка.</param>
        static void StartLightHouse(LightHouseCell LightHouse)
        {
            lightHouse.IsActive = true;
            lightHouse.Lighthouse = LightHouse;
            IEnumerable<Cell> closedCells = Map.Where(cell => !cell.IsOpened);
            lightHouse.SelectedCells = new(Math.Min(closedCells.Count(), 4));
            foreach (Cell cell in closedCells)
                cell.CanBeSelected = true;
        }
        /// <summary>
        /// Метод запуска хода конопли.
        /// </summary>
        static void StartCannabis()
        {
            Pirate[] bufferPirates = Players[^1].Pirates.ToArray();
            ShipCell bufferShip = Players[^1].ManagedShip;

            for (int i = Players.Count - 1; i > 0; i--)
                Players[i].SetPiratesAndShip(Players[i - 1], true);
            Players[0].SetPiratesAndShip(bufferPirates, bufferShip, true);

            NextPlayer(checkOnly: true);
            CurrentPlayer.Turn = false;
            CurrentPlayerNumber++;
            CurrentPlayer.CannabisStarter = true;
            CurrentPlayerNumber++;
            CurrentPlayer.Turn = true;
        }
        /// <summary>
        /// Метод окончания хода конопли.
        /// </summary>
        static void EndCannabis()
        {
            Pirate[] bufferPirates = Players[0].Pirates.ToArray();
            ShipCell bufferShip = Players[0].ManagedShip;

            bool blockRum = Players.Count(player => player.CannabisStarter) > 1;
            for (int i = 0; i < Players.Count - 1; i++)
                Players[i].SetPiratesAndShip(Players[i + 1], blockRum);
            Players[^1].SetPiratesAndShip(bufferPirates, bufferShip, blockRum);

            CurrentPlayer.CannabisStarter = false;
            NextPlayer(checkOnly: true);
        }

        /// <summary>
        /// Метод спаивания выбранного пирата.
        /// </summary>
        public static void GetPirateDrunk()
        {
            FileHandler.DrinkRum(SelectedPirate, ResidentType.Ben);

            PirateIsDrunk = true;
            CurrentPlayer.Bottles--;
            Deselect(false);
            SelectedPirate.IsDrunk = true;
            SelectPirate(SelectedPirate);
        }
        /// <summary>
        /// Метод спаивания Пятницы около выбранного пирата.
        /// </summary>
        public static void GetFridayDrunk()
        {
            FileHandler.DrinkRum(SelectedPirate, ResidentType.Friday);

            CurrentPlayer.Bottles--;
            foreach (Pirate pirate in CurrentPlayer.Pirates)
                pirate.CanGiveRumToFriday = false;

            if (SelectedPirate.Cell.Pirates.First(p => p is Friday) is Friday friday)
                friday.Kill();
            else
            {
                foreach (Cell cell in Map.Cells(SelectedPirate))
                {
                    if (cell.Pirates.FirstOrDefault(p => p is Friday) is Friday _friday)
                    {
                        _friday.Kill();
                        break;
                    }
                }
            }
            Deselect();
        }
        /// <summary>
        /// Метод спаивания Миссионера около выбранного пирата.
        /// </summary>
        public static void GetMissionerDrunk()
        {
            FileHandler.DrinkRum(SelectedPirate, ResidentType.Missioner);

            CurrentPlayer.Bottles--;
            foreach (Pirate pirate in CurrentPlayer.Pirates)
                pirate.CanGiveRumToMissioner = false;

            if (SelectedPirate.Cell.Pirates.First(p => p is Missioner) is Missioner missioner)
                missioner.ConverToPirate();
            else
            {
                foreach (Cell cell in Map.Cells(SelectedPirate))
                {
                    if (cell.Pirates.FirstOrDefault(p => p is Missioner) is Missioner _missioner)
                    {
                        _missioner.ConverToPirate();
                        break;
                    }
                }
            }
            Deselect();
        }

        /// <summary>
        /// Метод рождения пирата.
        /// </summary>
        public static void PirateBirth()
        {
            FileHandler.GetBirth(SelectedPirate);

            SelectedPirate.GiveBirth();
            Deselect();
            NextPlayer();
        }


        /// <summary>
        /// Метод приоткрывает закрытые клетки поля.
        /// </summary>
        public static void ShowField()
        {
            foreach (Cell cell in Map.Where(cell => !cell.IsOpened))
            {
                cell.IsPreOpened = !cell.IsPreOpened;
                cell.ChangeGrayStatus();
            }
        }
    }
}
