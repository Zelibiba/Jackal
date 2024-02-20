using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Drawing.Drawing2D;
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
        /// Количество миллисекунд анимации пирата.
        /// </summary>
        public static readonly int pirateDelay = 300;

        /// <summary>
        /// Карта.
        /// </summary>
        /// <remarks>
        /// Представляяет собой массив клеток типа <see cref="Cell"/>.
        /// </remarks>
        public static Map Map { get; private set; }

        /// <summary>
        /// Лист игроков.
        /// </summary>
        public static ObservableCollection<Player> Players { get; } = new();
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
        /// Список команд игроков.
        /// </summary>
        static readonly List<List<Player>> _ListAllies = new();
        /// <summary>
        /// Список команд игроков, упорядоченных по накопленному командой золоту по убыванию.
        /// </summary>
        /// <remarks>Целое значение Item1 является золотом, накопленным командой.</remarks>
        static readonly IEnumerable<(int, List<Player>)> _orderedAllies = from allies in _ListAllies
                                                                          orderby allies.Sum(player => player.Gold) descending
                                                                          select (allies.Sum(player => player.Gold), allies);

        /// <summary>
        /// Коллекция всех пиратов в игре.
        /// </summary>
        /// <remarks>Необходима для их отображения в интерфейсе.</remarks>
        public static ObservableCollection<Pirate> Pirates { get; } = new();

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
        /// Делегат для обновления параметров <see cref="HiddenGold"/>, <see cref="CurrentGold"/> и <see cref="LostGold"/> в интерфейсе.
        /// </summary>
        public static Action? UpdateHiddenParams;
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
        static int __hiddenGold = 0;
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
        public static int HiddenBottles { get; set; } = 0;

        /// <summary>
        /// Метод инициализирует игру.
        /// </summary>
        /// <param name="players">Упорядоченный список игроков.</param>
        /// <param name="seed">Сид для генерации карты.</param>
        /// <param name="autosave">Флаг того, что необходимо включить автосохранения.</param>
        public static void CreateMap(IEnumerable<Player> players, GameProperties properties, bool autosave = true)
        {
            #region инициализация игроков с командами
            foreach (Player player in players)
            {
                Players.Add(player);

                List<Player>? allies = _ListAllies.FirstOrDefault(allies => allies[0].AllianceIdentifier == player.AllianceIdentifier &&
                                                                            player.AllianceIdentifier != AllianceIdentifier.None);
                if (allies == null)
                    _ListAllies.Add(new List<Player> { player });
                else
                    allies.Add(player);
            }

            foreach (List<Player> allies in _ListAllies)
            {
                Team alliance = Team.None;
                foreach (Player player in allies)
                    alliance |= player.Team;
                foreach (Player player in allies)
                    player.SetAlliance(alliance, allies);
            }
            #endregion

            #region создание карты
            Map = new Map(properties);
            List<Coordinates> coords = Map.GroundCoordinates();
            List<string> names = new(coords.Count);
            List<string> pattern = new();

            // учёт гарантированных клеток и подготовка вероятностных клеток
            foreach ((string name, (int count, char param)) in properties.MapPattern)
            {
                // развертывание типа стрелок
                string[] cells;
                if (name == "Arrow")
                {
                    cells = properties.MapType == MapType.Quadratic ?
                        new string[] { "q1a", "q1s", "q2a", "q2s", "q3", "q4a", "q4s" } :
                        new string[] { "h1", "h2a", "h2b", "h3a", "h3b", "h3c" };
                    for (int i = 0; i < cells.Length; i++)
                        cells[i] = "Arrow_" + cells[i];
                }
                else
                    cells = new string[] { name };

                foreach (string _name in cells)
                {
                    string[] massive = new string[count];
                    for (int i = 0; i < count; i++)
                        massive[i] = _name;
                    switch (param)
                    {
                        case ' ': pattern.AddRange(massive); break;
                        case '=': names.AddRange(massive); break;
                        case '>':
                            names.AddRange(massive);
                            pattern.AddRange(massive); break;
                        default: throw new ArgumentException(param.ToString());
                    }
                }
            }

            // учёт вероятностных клеток
            Random rand = new(properties.Seed);
            while (names.Count != names.Capacity)
                names.Add(pattern[rand.Next(pattern.Count)]);

            // проверка на одиночную пещеру
            if (names.Count(x => x == "Cave") == 1)
            {
                for (int i = names.Count - 1; i >= 0; i--)
                {
                    if (names[i] != "Cave")
                    {
                        names[i] = "Cave";
                        break;
                    }
                }
            }

            List<CaveCell> caveCells = new();
            foreach(Coordinates coord in coords)
            {
                int row = coord.Row;
                int column = coord.Column;
                int index = rand.Next(names.Count);
                string name = names[index];
                names.RemoveAt(index);
                switch (name.Split('_')[0])
                {
                    case "Field":      Map.SetCell(new Cell(row, column, "Field")); break;
                    case "Horse":      Map.SetCell(new HorseCell(row, column)); break;
                    case "Rum":        Map.SetCell(new RumCell(row, column)); break;
                    case "Lake":       Map.SetCell(new LakeCell(row, column, ContinueMovePirate)); break;
                    case "Pit":        Map.SetCell(new PitCell(row, column)); break;
                    case "Crocodile":  Map.SetCell(new CrocodileCell(row, column, ContinueMovePirate)); break;
                    case "Fortress":   Map.SetCell(new FortressCell(row, column, false)); break;
                    case "Balloon":    Map.SetCell(new BalloonCell(row, column, ContinueMovePirate)); break;
                    case "Jungle":     Map.SetCell(new JungleCell(row, column)); break;
                    case "Cannabis":   Map.SetCell(new CannabisCell(row, column)); break;
                    case "Cannibal":   Map.SetCell(new CannibalCell(row, column)); break;
                    case "Putana":     Map.SetCell(new FortressCell(row, column, true)); break;
                    case "Airplane":   Map.SetCell(new AirplaneCell(row, column)); break;
                    case "Friday":     Map.SetCell(new ResidentCell(row, column, ResidentType.Friday)); break;
                    case "Missioner":  Map.SetCell(new ResidentCell(row, column, ResidentType.Missioner)); break;
                    case "Ben":        Map.SetCell(new ResidentCell(row, column, ResidentType.Ben)); break;
                    case "EarthQuake": Map.SetCell(new EarthQuakeCell(row, column)); break;
                    case "Carramba":   Map.SetCell(new Cell(row, column, "Field")); break;
                    case "LightHouse": Map.SetCell(new LightHouseCell(row, column)); break;
                    case "Cave":
                        Map.SetCell(new CaveCell(row, column, ContinueMovePirate));
                        caveCells.Add(Map[row, column] as CaveCell); break;
                    case "Gun":
                        int rotation = rand.Next(Map.Type == MapType.Quadratic ? 4 : 6);
                        Map.SetCell(new GunCell(row, column, rotation, ContinueMovePirate)); break;
                    case "Arrow":
                        rotation = rand.Next(Map.Type == MapType.Quadratic ? 4 : 6);
                        ArrowType arrowType = name.Split('_')[1] switch
                        {
                            "q1a" => ArrowType.Angle1,
                            "q1s" => ArrowType.Side1,
                            "q2a" => ArrowType.Angle2,
                            "q2s" => ArrowType.Side2,
                            "q3"  => ArrowType.Angle3,
                            "q4a" => ArrowType.Angle4,
                            "q4s" => ArrowType.Side4,
                            "h1" => ArrowType.Hex1,
                            "h2a" => ArrowType.Hex2a,
                            "h2b" => ArrowType.Hex2b,
                            "h3a" => ArrowType.Hex3a,
                            "h3b" => ArrowType.Hex3b,
                            "h3c" => ArrowType.Hex3c,
                            "h4a" => ArrowType.Hex4a,
                            "h4b" => ArrowType.Hex4b,
                            _ => throw new Exception("Wrong random ArrowType")
                        };
                        Map.SetCell(new ArrowCell(row, column, arrowType, rotation, ContinueMovePirate)); break;
                    case "Maze":
                        int size = int.Parse(name.Split('_')[1]);
                        Map.SetCell(new MazeCell(row, column, size));break;
                    case "Bottle":
                        int count = int.Parse(name.Split('_')[1]);
                        Map.SetCell(new BottleCell(row, column, count));
                        HiddenBottles += count; break;
                    case "Galeon": 
                        Map.SetCell(new GoldCell(row, column, GoldType.Galeon));
                        __hiddenGold += 3; break;
                    case "Gold":
                        GoldType gold = name.Split('_')[1] switch
                        {
                            "1" => GoldType.Gold1,
                            "2" => GoldType.Gold2,
                            "3" => GoldType.Gold3,
                            "4" => GoldType.Gold4,
                            "5" => GoldType.Gold5,
                            _ => throw new Exception("Wrong random Gold Type")
                        };
                        Map.SetCell(new GoldCell(row, column, gold));
                        __hiddenGold += (int)gold; break;
                    default: throw new Exception("Wrong random cell");
                }
            }
            foreach (CaveCell cave in caveCells)
                cave.LinkCaves(caveCells);
            #endregion

            #region Создание кораблей
            if (Players.Count == 3 && properties.MapType == MapType.Quadratic)
            {
                Map.ShipPlacements[1].InitialCoordinates = new((properties.Size + 1) * 2 / 3, 0);
                Map.ShipPlacements[3].InitialCoordinates = new((properties.Size + 1) * 2 / 3, properties.Size + 1);
            }
            Map.SetShipToPlayer(0, Players[0]);
            if (properties.MapType == MapType.Quadratic)
            {
                switch (Players.Count)
                {
                    case 2: Map.SetShipToPlayer(2, Players[1]); break;
                    case 3: Map.SetShipToPlayer(1, Players[1]);
                            Map.SetShipToPlayer(3, Players[2]); break;
                    case 4: Map.SetShipToPlayer(1, Players[1]);
                            Map.SetShipToPlayer(2, Players[2]);
                            Map.SetShipToPlayer(3, Players[3]); break;
                }
            }
            else
            {
                switch (Players.Count)
                {
                    case 2: Map.SetShipToPlayer(3, Players[1]); break;
                    case 3: Map.SetShipToPlayer(2, Players[1]);
                            Map.SetShipToPlayer(4, Players[2]); break;
                    case 4: Map.SetShipToPlayer(1, Players[1]);
                            Map.SetShipToPlayer(3, Players[2]);
                            Map.SetShipToPlayer(4, Players[3]); break;
                    case 5: Map.SetShipToPlayer(1, Players[1]);
                            Map.SetShipToPlayer(2, Players[2]);
                            Map.SetShipToPlayer(4, Players[3]);
                            Map.SetShipToPlayer(5, Players[4]); break;
                    case 6: Map.SetShipToPlayer(1, Players[1]);
                            Map.SetShipToPlayer(2, Players[2]);
                            Map.SetShipToPlayer(3, Players[3]);
                            Map.SetShipToPlayer(4, Players[4]);
                            Map.SetShipToPlayer(5, Players[5]); break;
                }
            }
            #endregion

            Map.SetCell(new EarthQuakeCell(2,2));
            //Map.SetCell(new CannabisCell(11, 6));
            //Map[1, 5].AddPirate(new Missioner(Map[1, 5], Players[1], Players[1]));

            foreach (Cell cell in Map)
                cell.SetSelectableCoords(Map);

            if (autosave)
                SaveOperator.StartAutosave(Players, properties);

            CurrentPlayerNumber = Players.Count - 1;
            NextPlayer();
        }
        /// <summary>
        ///Метод совершает все ходы игры, записанные в списке операций.
        /// </summary>
        /// <param name="operations">Список загружаемых операций.</param>
        public static void ReadOperations(IEnumerable<int[]> operations)
        {
            foreach (int[] operation in operations)
            {
                switch ((Actions)operation[0])
                {
                    case Actions.MovePirate:
                        int index = operation[1];
                        bool gold = operation[2] == 1;
                        bool galeon = operation[3] == 1;
                        Coordinates coords = new(operation[4], operation[5]);
                        SelectPirate(index, gold, galeon, coords);
                        SelectCell(coords); break;
                    case Actions.MoveShip:
                        SelectCell(CurrentPlayer.ManagedShip);
                        SelectCell(new Coordinates(operation[1], operation[2])); break;
                    case Actions.CellSelection:
                        SelectCell(new Coordinates(operation[1], operation[2])); break;
                    case Actions.DrinkRum:
                        index = operation[1];
                        ResidentType type = (ResidentType)operation[2];
                        SelectPirate(index);
                        GetDrunk(type); break;
                    case Actions.GetBirth:
                        index = operation[1];
                        SelectPirate(index);
                        PirateBirth(); break;
                }
            }
        }




        /// <summary>
        /// Метод передаёт ход следующему игроку.
        /// </summary>
        /// <param name="checkOnly">Флаг того, что необходима только проверка на бочку рома.</param>
        static void NextPlayer()
        {
            UpdateHiddenParams?.Invoke();
            CheckRumBlock();

            if (CurrentPlayer.CannabisIndex > 0)
                EndCannabis();
            // Из-за того, что в любом случае должна пройти проверка на невозможность хода.
            else
            {
                CurrentPlayer.Turn = false;
                CurrentPlayerNumber++;
                CurrentPlayer.Turn = true;

                SelectedPirate = null;
                SelectedShip = null;

                // проверка на возможность споить миссионера или пятницу
                foreach (Pirate pirate in CurrentPlayer.Pirates)
                    pirate.DefineDrinkingOpportynities(Map);

                #region Проверка на победителя
                if (!_hasWinner && _ListAllies.Count > 1)
                {
                    (int, List<Player>)[] orderedAllies = _orderedAllies.ToArray();
                    if (orderedAllies[0].Item1 > orderedAllies[1].Item1 + CurrentGold + HiddenGold)
                    {
                        _hasWinner = true;
                        Dispatcher.UIThread.InvokeAsync(() => SetWinner?.Invoke(orderedAllies[0].Item2)).Wait();
                    }
                }
                #endregion
            }

            // если нет доступных ходов - переход к другому игроку
            if (!CurrentPlayer.Pirates.Any(pirate => !pirate.IsBlocked ||
                                                     pirate.IsBlocked && pirate.CanDrinkRum))
                NextPlayer();
        }
        /// <summary>
        /// Метод обрабатывает пропуск хода из-за бочки с ромом.
        /// </summary>
        static void CheckRumBlock()
        {
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
        }

        /// <summary>
        /// Делегат для оповещения о назначении победителя.
        /// </summary>
        public static Action<IEnumerable<Player>>? SetWinner;
        /// <summary>
        /// Флаг того, что победитель уже есть.
        /// </summary>
        static bool _hasWinner;


        /// <summary>
        /// Делегат для влючения и отключения интерфеса.
        /// </summary>
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
            else if (SelectedShip != null)
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
        /// <remarks>Необходима для <see cref="SaveOperator.ReadSave(string)"/> и <see cref="Client"/>.</remarks>
        /// <param name="coordinates">Координаты выбранной ячейки.</param>
        public static void SelectCell(Coordinates coordinates) => SelectCell(Map[coordinates]);
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
            SaveOperator.EarthQuake(cell);
            Client.SelectCell(NetMode.EathQuake, cell);

            if (earthQuake.SelectedCell == null)
                earthQuake.SelectCell(cell);
            else if (cell.IsSelected)
                earthQuake.DeselectCell();
            else
            {
                Deselect();
                LogWriter.EarthQuake(earthQuake.SelectedCell, cell);
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
            SaveOperator.LightHouse(cell);
            Client.SelectCell(NetMode.LightHouse, cell);

            if (lightHouse.SelectedCells.Count < lightHouse.SelectedCells.Capacity)
            {
                cell.CanBeSelected = false;
                cell.IsSelected = true;
                if (CurrentPlayer.IsControllable)
                    cell.IsPreOpened = true;
                else
                    cell.IsLightHousePicked = true;
                lightHouse.SelectedCells.Add(cell);
                if (lightHouse.SelectedCells.Count == lightHouse.SelectedCells.Capacity)
                {
                    Deselect();
                    foreach (Cell c in lightHouse.SelectedCells)
                    {
                        if (CurrentPlayer.IsControllable)
                            c.CanBeSelected = true;
                        c.IsSelected = false;
                    }
                    if (CurrentPlayer.IsControllable)
                        lightHouse.Lighthouse.CanBeSelected = true;

                }
            }
            else
            {
                if (cell == lightHouse.Lighthouse)
                {
                    cell.CanBeSelected = false;
                    foreach (Cell c in lightHouse.SelectedCells)
                    {
                        c.IsSelected = false;
                        if (CurrentPlayer.IsControllable)
                        {
                            c.CanBeSelected = false;
                            c.ChangeGrayStatus(stayGray: true);
                        }
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
            if (CurrentPlayer.IsControllable)
            {
                foreach (Cell cell in Map.Cells(pirate))
                    cell.DefineSelectability(pirate);
            }
        }
        /// <summary>
        /// Метод тихой выборки пирата без обработки интерфейса и достижимых координат.
        /// </summary>
        /// <remarks>Необходима для <see cref="SaveOperator.ReadSave(string)"/> и <see cref="Client"/>.</remarks>
        /// <param name="pirateIndex">Индекс пирата в списке пиратов у игрока.</param>
        /// <param name="gold">Флаг того, что пират понесёт золото.</param>
        /// <param name="galeon">Флаг того, что пират понесёт Галеон.</param>
        /// <param name="targetCoords">Координаты ячейки, куда направляется пират.</param>
        public static void SelectPirate(int pirateIndex, bool gold = false ,bool galeon = false, Coordinates? targetCoords = null)
        {
            SelectedPirate = CurrentPlayer.Pirates[pirateIndex];
            SelectedPirate.Gold = gold;
            SelectedPirate.Galeon = galeon;
            if (targetCoords != null)
                Map[targetCoords].GetSelectedCell(SelectedPirate).CanBeSelected = true;
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
                    cell.CanBeSelected = false;

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
        public static Action<int, int, bool>? StartPirateAnimation;
        /// <summary>
        /// Метод запуска анимации перемещения пирата.
        /// </summary>
        /// <param name="pirate">Перемещаемый пират.</param>
        /// <param name="cell">Клетка, куда перемещается пират.</param>
        /// <param name="delay">Задержка в миллисекундах для синхронизации с анимацией.</param>
        /// <param name="kill">Флаг того, что пират убит, и нужно очистить за ним.</param>
        public static void OnStartPirateAnimation(Pirate pirate, Cell cell, int delay, bool kill = false)
        {
            if (StartPirateAnimation != null)
            {
                cell.IsPreOpened = true;
                int pirateIndex = Pirates.IndexOf(pirate);
                int cellIndex = Map.IndexOf(cell);
                StartPirateAnimation(pirateIndex, cellIndex, kill);
                if (delay > 0)
                    Task.Delay(delay).Wait();
            }
        }
        /// <summary>
        /// Метод обработки перемещения пирата.
        /// </summary>
        /// <param name="cell">Клетка, куда перемещается пират.</param>
        static void StartMovePirate(Cell cell)
        {
            LogWriter.MovePirate(SelectedPirate, cell);
            SaveOperator.MovePirate(SelectedPirate, cell);
            Client.MovePirate(SelectedPirate, cell);

            SelectedPirate.TargetCell = cell;
            if (!PirateInMotion)
            {
                PirateInMotion = true;
                SelectedPirate.PrepareToMove();
                if (PirateIsDrunk)
                    PirateIsDrunk = false;
            }

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
                    bool result = StartEarthQuake();
                    PirateInMotion = false;
                    if (!result) NextPlayer();
                    break;
                case MovementResult.LightHouse:
                    LightHouseCell lightHouse = SelectedPirate.Cell as LightHouseCell;
                    SelectedPirate = null;
                    result = StartLightHouse(lightHouse);
                    PirateInMotion = false;
                    if (!result) NextPlayer();
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
        static MovementResult ContinueMovePirate(Coordinates coords)
        {
            Cell cell = Map[coords].GetSelectedCell(SelectedPirate);

            if (!cell.IsGoldFriendly(SelectedPirate))
            {
                if (SelectedPirate.Gold)
                    SelectedPirate.Gold = false;
                if (SelectedPirate.Galeon)
                    SelectedPirate.Galeon = false;
            }
            if(!cell.CanBeSelectedBy(SelectedPirate))
            {
                OnStartPirateAnimation(SelectedPirate, cell, pirateDelay);
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
            return cell.AddPirate(SelectedPirate, delay: pirateDelay);
        }


        /// <summary>
        /// Делегат для управления z-индексом клеток при их перемещении.
        /// </summary>
        public static Action<int, int, int, int>? SetCellZIndex;
        /// <summary>
        /// Вызов делегата <see cref="SetCellZIndex"/>.
        /// </summary>
        /// <param name="cell1">Одна клетка.</param>
        /// <param name="cell2">Вторая клетка.</param>
        /// <param name="start">Флаг того, начало ли перемещения или конц.</param>
        static void OnSetCellZIndex(Cell cell1, Cell cell2, bool start)
        {
            if (SetCellZIndex != null)
            {
                int cell1Index = Map.IndexOf(cell1);
                int cell2Index = Map.IndexOf(cell2);
                int zIndex1, zIndex2;
                if (start)
                {
                    zIndex1 = cell1 is SeaCell ? 0 : 100;
                    zIndex2 = cell2 is SeaCell ? 0 : 100;
                }
                else
                {
                    zIndex1 = 1;
                    zIndex2 = 1;
                }
                SetCellZIndex(cell1Index, zIndex1, cell2Index, zIndex2);
            }
        }
        /// <summary>
        /// Метод перемещения корабля.
        /// </summary>
        /// <param name="newCell">Клетка, на которую перемещается корабль.</param>
        static void MoveShip(Cell cell)
        {
            LogWriter.MoveShip(cell);
            SaveOperator.MoveShip(cell);
            Client.SelectCell(NetMode.MoveShip, cell);

            // Подобрать или убить пиратов в море
            if (cell.Pirates.Count > 0)
            {
                if (SelectedShip.IsFriendlyTo(cell.Pirates[0]))
                {
                    foreach(Pirate pirate in cell.Pirates)
                        SelectedShip.AddPirate(pirate);
                    cell.Pirates.Clear();
                }
                else
                {
                    while(cell.Pirates.Count > 0)
                        cell.Pirates[0].Kill();
                }
            }

            Deselect(false);
            SwapCells(SelectedShip, cell);

            SelectedShip = null;
            NextPlayer();
        }
        /// <summary>
        /// Метод для перемены двух клеток местами.
        /// </summary>
        /// <param name="cell1">Первая перемещаемая клетка.</param>
        /// <param name="cell2">Вторая перемещаемая клетка.</param>
        /// <param name="animation">Флаг того, что перемещение ячеек нужно анимировать.</param>
        static void SwapCells(Cell cell1, Cell cell2)
        {
            Coordinates coords1 = cell1.Coords;
            Coordinates coords2 = cell2.Coords;

            OnSetCellZIndex(cell1, cell2, true);

            Map.SwapIndexes(coords1, coords2);
            cell2.SetCoordinates(coords1.Row, coords1.Column);
            cell1.SetCoordinates(coords2.Row, coords2.Column);
            cell1.SetSelectableCoords(Map);
            cell2.SetSelectableCoords(Map);

            Task.Delay(500).Wait();
            OnSetCellZIndex(cell1, cell2, false);
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
        /// <returns>True, если есть хотя бы 2 подходящих клетки.</returns>
        static bool StartEarthQuake()
        {
            Cell[] relevantCells = Map.Where(cell => cell is not SeaCell && cell is not ShipCell &&
                                                     cell.Gold == 0 && cell.Galeon == 0 &&
                                                     cell.AllPiratesCount() == 0)
                                      .ToArray();
            if (relevantCells.Length < 2)
                return false;

            earthQuake.IsActive = true;
            if (CurrentPlayer.IsControllable)
            {
                foreach (Cell cell in relevantCells)
                    cell.CanBeSelected = true;
            }
            return true;
        }
        /// <summary>
        /// Метод запуска хода маяка.
        /// </summary>
        /// <param name="LightHouse">Клетка маяка, вызвавшая этот ход маяка.</param>
        /// <returns>True, если есть хотя бы одна неоктрытая клетка.</returns>
        static bool StartLightHouse(LightHouseCell LightHouse)
        {
            Cell[] closedCells = Map.Where(cell => !cell.IsOpened).ToArray();
            if (closedCells.Length == 0)
                return false;

            lightHouse.IsActive = true;
            lightHouse.Lighthouse = LightHouse;
            lightHouse.SelectedCells = new(Math.Min(closedCells.Length, 4));
            if (CurrentPlayer.IsControllable)
            {
                foreach (Cell cell in closedCells)
                    cell.CanBeSelected = true;
            }
            return true;
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

            CheckRumBlock();
            CurrentPlayer.Turn = false;
            CurrentPlayerNumber++;
            CurrentPlayer.CannabisIndex++;
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

            bool blockRum = Players.Count(player => player.CannabisIndex > 0) > 1;
            for (int i = 0; i < Players.Count - 1; i++)
                Players[i].SetPiratesAndShip(Players[i + 1], blockRum);
            Players[^1].SetPiratesAndShip(bufferPirates, bufferShip, blockRum);

            CurrentPlayer.CannabisIndex--;
        }

        /// <summary>
        /// Метод спаивания какого-либо юнита.
        /// </summary>
        /// <param name="type">Тип спаиваемого юнита. Для обычного пирата равен <see cref="ResidentType.Ben"/>.</param>
        public static void GetDrunk(ResidentType type)
        {
            LogWriter.DrinkRum(type);
            SaveOperator.DrinkRum(SelectedPirate, type);
            Client.DrinkRum(SelectedPirate, type);

            switch(type)
            {
                case ResidentType.Ben:
                    GetPirateDrunk(); break;
                case ResidentType.Friday:
                    GetFridayDrunk(); break;
                case ResidentType.Missioner:
                    GetMissionerDrunk(); break;
            }
        }
        /// <summary>
        /// Метод спаивания выбранного пирата.
        /// </summary>
        static void GetPirateDrunk()
        {
            PirateIsDrunk = true;
            CurrentPlayer.UseBottle();
            Deselect(false);
            SelectedPirate.IsDrunk = true;
            if (CurrentPlayer.IsControllable)
                SelectPirate(SelectedPirate);
        }
        /// <summary>
        /// Метод спаивания Пятницы около выбранного пирата.
        /// </summary>
        static void GetFridayDrunk()
        {
            CurrentPlayer.UseBottle();
            foreach (Pirate pirate in CurrentPlayer.Pirates)
                pirate.CanGiveRumToFriday = false;

            if (SelectedPirate.Cell.Pirates.FirstOrDefault(p => p is Friday, Pirate.Empty) is Friday friday)
                friday.Kill();
            else
            {
                foreach (Cell cell in Map.Cells(SelectedPirate))
                {
                    if (cell.Pirates.FirstOrDefault(p => p is Friday, Pirate.Empty) is Friday _friday)
                    {
                        _friday.Kill();
                        break;
                    }
                }
            }

            Deselect(false);
            if (CurrentPlayer.IsControllable)
                SelectPirate(SelectedPirate);
        }
        /// <summary>
        /// Метод спаивания Миссионера около выбранного пирата.
        /// </summary>
        static void GetMissionerDrunk()
        {
            CurrentPlayer.UseBottle();
            foreach (Pirate pirate in CurrentPlayer.Pirates)
                pirate.CanGiveRumToMissioner = false;

            if (SelectedPirate.Cell.Pirates.FirstOrDefault(p => p is Missioner, Pirate.Empty) is Missioner missioner)
                missioner.ConverToPirate();
            else
            {
                foreach (Cell cell in Map.Cells(SelectedPirate))
                {
                    if (cell.Pirates.FirstOrDefault(p => p is Missioner, Pirate.Empty) is Missioner _missioner)
                    {
                        _missioner.ConverToPirate();
                        break;
                    }
                }
            }

            Deselect(false);
            if (CurrentPlayer.IsControllable)
                SelectPirate(SelectedPirate);
        }

        /// <summary>
        /// Метод рождения пирата.
        /// </summary>
        public static void PirateBirth()
        {
            if (CurrentPlayer.IsControllable)
                EnableInterface?.Invoke(false);

            LogWriter.GetBirth();
            SaveOperator.GetBirth(SelectedPirate);
            Client.PirateBirth(SelectedPirate);

            SelectedPirate.GiveBirth();
            Deselect();
            NextPlayer();

            if (CurrentPlayer.IsControllable)
                EnableInterface?.Invoke(true);
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
            //FileHandler.ReadAutosave();
        }

        /// <summary>
        /// Делегат для написания лога о ходе.
        /// </summary>
        public static Action<string>? WriteLog;



    }
}
