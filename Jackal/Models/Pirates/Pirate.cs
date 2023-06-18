using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Jackal.Models.Cells;
using System.Reactive.Linq;

namespace Jackal.Models.Pirates
{
    /// <summary>
    /// Класс пирата.
    /// </summary>
    public class Pirate : ReactiveObject
    {
        /// <summary>
        /// Заглушка пирата для корректной работы интерфейса.
        /// </summary>
        public static readonly Pirate Empty = new()
        {
            IsVisible = true,
            Owner = new Player(),
            Manager = new Player()
        };
        public Pirate() { }

        /// <summary>
        /// Конструктор пирата.
        /// </summary>
        /// <param name="owner">Игрок, который владеет данным пиратом.</param>
        /// <param name="manager">Игрок, который управляет данным пиратом.</param>
        /// <param name="image">Цвет изображения пирата.</param>
        /// <param name="isFighter">Флаг того, что пират может сражаться и управлять кораблём.</param>
        public Pirate(Player owner, Player? manager = null, string? image = null, bool isFighter = true)
        {
            Owner = owner;
            Manager = manager ?? owner;
            Manager.Pirates.Add(this);
            this.WhenAnyValue(p => p.Owner)
                .Select(owner => owner.Team)
                .ToPropertyEx(this, p => p.Team);


            Image = image ?? Team.ToString();
            IsVisible = true;
            IsEnabled = true;

            IsFighter = isFighter;
            _loopDict = new Dictionary<Cell, int>();

            this.WhenAnyValue(p => p.Gold)
                .Skip(1)
                .Where(x => !x)
                .Subscribe(x =>
                {
                    if (!Cell.IsStandable)
                    {
                        Cell.Gold--;
                        StartCell.Gold++;
                    }
                });
            this.WhenAnyValue(p => p.Galeon)
                .Skip(1)
                .Where(x => !x)
                .Subscribe(x =>
                {
                    if (!Cell.IsStandable)
                    {
                        Cell.Galeon = false;
                        StartCell.Galeon = true;
                    }
                });

            this.WhenAnyValue(p => p.Cell)
                .Skip(1)
                .Select(cell => cell is HorseCell || (cell is LakeCell && AtHorse))
                .ToPropertyEx(this, p => p.AtHorse);
            this.WhenAnyValue(p => p.Cell)
                .Skip(1)
                .Select(cell => cell is AirplaneCell airpane && airpane.IsActive
                                || (cell is LakeCell && AtAirplane))
                .ToPropertyEx(this, p => p.AtAirplane);

            this.WhenAnyValue(p => p.Cell)
                .Skip(1)
                .Where(cell => cell is ArrowCell arrowCell
                               && (arrowCell.ArrowType == ArrowType.Side1 || arrowCell.ArrowType == ArrowType.Angle1)
                               || cell is CrocodileCell)
                .Subscribe(cell =>
                {
                    if (_loopDict.ContainsKey(cell))
                    {
                        if (++_loopDict[cell] == 3)
                            IsInLoop = true;
                    }
                    else
                        _loopDict.Add(cell, 1);
                });

            _mazeNodeNumber = this.WhenAnyValue(p => p.Cell)
                                  .Skip(1)
                                  .Select(cell => cell.Number)
                                  .ToProperty(this, p => p.MazeNodeNumber);
        }

        /// <summary>
        /// Игрок, владеющий пиратом.
        /// </summary>
        [Reactive] public Player Owner { get; protected set; }
        /// <summary>
        /// Игрок, управляющий пиратом.
        /// </summary>
        public Player Manager { get; set; }
        /// <summary>
        /// Команда пирата.
        /// </summary>
        [ObservableAsProperty] public Team Team { get; }
        /// <summary>
        /// Альянс, в котором состоит пират.
        /// </summary>
        public Team Alliance => Owner.Alliance;

        /// <summary>
        /// Флаг того, что с пиратом можно взаимодействовать.
        /// </summary>
        /// <remarks>Используется в интерфейсе.</remarks>
        [Reactive] public bool IsEnabled { get; set; }
        /// <summary>
        /// Флаг того, что пират выбран.
        /// </summary>
        /// <remarks>
        /// Необходим для интерфейса.
        /// </remarks>
        [Reactive] public bool IsSelected { get; set; }
        /// <summary>
        /// Флаг того, что пират виден.
        /// </summary>
        /// /// <remarks>
        /// Необходим для интерфейса.
        /// </remarks>
        [Reactive] public bool IsVisible { get; set; }
        /// <summary>
        /// Цвет изображения пирата.
        /// </summary>
        public string Image { get; }

        /// <summary>
        /// Клетка, на которой находится пират.
        /// </summary>
        [Reactive] public Cell Cell { get; set; }
        /// <summary>
        /// Строка клетки с пиратом.
        /// </summary>
        public int Row => Cell.Row;
        /// <summary>
        /// Колонка клетки с пиратом.
        /// </summary>
        public int Column => Cell.Column;
        /// <summary>
        /// Клетка, куда движется пират.
        /// </summary>
        public Cell TargetCell { get; set; }
        /// <summary>
        /// Клетка, с которой пират начинает движение.
        /// </summary>
        public Cell StartCell { get; protected set; }
        /// <summary>
        /// Метод задаёт <see cref="StartCell"/>.
        /// </summary>
        public void SetStartCell() => StartCell = Cell;

        /// <summary>
        /// Список координат ячеек, куда пират может пойти.
        /// </summary>
        /// <remarks>
        /// Имеет логику.
        /// </remarks>
        public virtual List<int[]> SelectableCoords
        {
            get
            {
                if (IsDrunk)
                    return (Cell as ITrapCell).AltSelectableCoords;
                return Cell.SelectableCoords;
            }
        }

        /// <summary>
        /// Словарь с ячейками, которые могут потенциально ввести в петлю.
        /// </summary>
        readonly Dictionary<Cell, int> _loopDict;
        /// <summary>
        /// Флаг того, что пират попал в петлю.
        /// </summary>
        public bool IsInLoop { get; protected set; }

        /// <summary>
        /// Флаг того, что пират перемещается конём.
        /// </summary>
        /// <remarks>
        /// Необходим для корректной работы озера.
        /// </remarks>
        [ObservableAsProperty] public bool AtHorse { get; }
        /// <summary>
        /// Флаг того, что пират перемещается самолётом.
        /// </summary>
        /// <remarks>
        /// Необходим для корректной работы озера.
        /// </remarks>
        [ObservableAsProperty] public bool AtAirplane { get; }
        /// <summary>
        /// Номер уровня лабиринта, на котором находится пират.
        /// </summary>
        public int MazeNodeNumber => _mazeNodeNumber?.Value ?? 0;
        readonly ObservableAsPropertyHelper<int> _mazeNodeNumber;

        /// <summary>
        /// Флаг того, что пират перемещает монету.
        /// </summary>
        [Reactive] public bool Gold { get; set; }
        /// <summary>
        /// Флаг того, что пират перемещает Галеон.
        /// </summary>
        [Reactive] public bool Galeon { get; set; }
        /// <summary>
        /// Флаг того, что пират перемещает сокровище.
        /// </summary>
        public bool Treasure => Gold || Galeon;
        /// <summary>
        /// Флаг того, что пират может взять золото.
        /// </summary>
        public virtual bool CanGrabGold => Cell?.Gold > 0;
        /// <summary>
        /// Флаг того, что пират может взять Галеон.
        /// </summary>
        public virtual bool CanGrabGaleon => Cell?.Galeon ?? false;

        /// <summary>
        /// Флаг того, что пират может сражаться и управлять кораблём.
        /// </summary>
        public bool IsFighter { get; }

        /// <summary>
        /// Флаг того, что пират может выпить ром.
        /// </summary>
        public virtual bool CanDrinkRum => Cell is ITrapCell && Manager.Bottles > 0 && !Manager.IsRumBlocked;
        /// <summary>
        /// Флаг того, что пират выпил ром.
        /// </summary>
        public bool IsDrunk { get; set; }
        /// <summary>
        /// Флаг того, что пират может споить Пятницу.
        /// </summary>
        public bool CanGiveRumToFriday { get; set; }
        /// <summary>
        /// Флаг того, что пират может споить Миссионера.
        /// </summary>
        public bool CanGiveRumToMissioner { get; set; }
        /// <summary>
        /// Метод определяет параметры <see cref="CanGiveRumToFriday"/> и <see cref="CanGiveRumToMissioner"/>.
        /// </summary>
        /// <param name="map">Карта игры.</param>
        public void DefineDrinkingOpportynities(ObservableMap map)
        {
            CanGiveRumToFriday = false;
            CanGiveRumToMissioner = false;

            if (this is Friday || this is Missioner || Manager.Bottles == 0 || Manager.IsRumBlocked)
                return;

            foreach (Pirate pirate in Cell.Pirates)
            {
                if (pirate is Friday)
                    CanGiveRumToFriday = true;
                else if (pirate is Missioner)
                    CanGiveRumToMissioner = true;
            }
            foreach (Cell cell in map.Cells(SelectableCoords))
            {
                foreach(Pirate pirate in cell.GetSelectedCell(this).Pirates)
                {
                    if (pirate is Friday)
                        CanGiveRumToFriday = true;
                    else if (pirate is Missioner)
                        CanGiveRumToMissioner = true;
                }
            }
        }

        /// <summary>
        /// Флаг того, что пират может родить пирата.
        /// </summary>
        public virtual bool CanHaveSex => !Manager.IsEnoughtPirates && Cell is FortressCell fortress && fortress.Putana;
        /// <summary>
        /// Метод рождает нового пирата рядом с данным пиратом.
        /// </summary>
        public void GiveBirth() => Cell.AddPirate(new Pirate(Owner, Manager));

        /// <summary>
        /// Флаг того, что пират заблокирован в яме или пещере.
        /// </summary>
        /// <remarks>Используется в интерфейсе.</remarks>
        [Reactive] public virtual bool IsBlocked { get; set; }
        /// <summary>
        /// Счётчик ходов, которые осталось пропустить из-за бочки с ромом.
        /// </summary>
        public int RumCount { get; set; }

        /// <summary>
        /// Метод удаляет пирата с клетки, на которой он находится.
        /// </summary>
        public void RemoveFromCell()
        {
            if (IsDrunk)
                IsDrunk = false;
            Cell.RemovePirate(this);
        }

        /// <summary>
        /// Метод убивает пирата.
        /// </summary>
        public void Kill()
        {
            TargetCell = null;
            Cell.RemovePirate(this, withGold: false);
            Manager.Pirates.Remove(this);
        }
        public void LoopKill()
        {
            Kill();
            if (Cell.Gold > 0)
            {
                Game.LostGold++;
                Cell.Gold = 0;
            }
            else if(Cell.Galeon)
            {
                Game.LostGold += 3;
                Cell.Galeon = false;
            }
        }
    }
}
