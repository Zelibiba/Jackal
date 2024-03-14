using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Jackal.Models.Cells;
using System.Reactive.Linq;
using System.Runtime.CompilerServices;
using System.Reactive;
using Jackal.Models.Cells.Utilites;

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
            Owner = new Player(),
            Manager = new Player()
        };
        public Pirate() { }

        /// <summary>
        /// Конструктор пирата.
        /// </summary>
        /// <param name="cell">Клетка, на которой располагается создаваемый пират.</param>
        /// <param name="owner">Игрок, который владеет данным пиратом.</param>
        /// <param name="manager">Игрок, который управляет данным пиратом.</param>
        /// <param name="image">Цвет изображения пирата.</param>
        /// <param name="isFighter">Флаг того, что пират может сражаться и управлять кораблём.</param>
        public Pirate(Cell cell, Player owner, Player? manager = null, string? image = null, bool isFighter = true)
        {
            IsFighter = isFighter;
            Owner = owner;
            Manager = manager ?? owner;
            Manager.Pirates.Add(this);
            this.WhenAnyValue(p => p.Owner)
                .Select(owner => owner.Team)
                .ToPropertyEx(this, p => p.Team);


            Image = image ?? Team.ToString();
            IsEnabled = true;

            _loopDict = new Dictionary<Cell, int>();

            

            this.WhenAnyValue(p => p.Cell.Gold)
                .Select(gold => gold > 0)
                .ToPropertyEx(this, p => p.CanGrabGold);
            this.WhenAnyValue(p => p.Cell.Galeon)
                .Select(galeon => galeon > 0)
                .ToPropertyEx(this, p => p.CanGrabGaleon);

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
                        Cell.Galeon--;
                        StartCell.Galeon++;
                    }
                });

            this.WhenAnyValue(p => p.Cell)
                .Skip(1)
                .Select(cell => cell is HorseCell || (cell is LakeCell && AtHorse) || (cell is CrocodileCell && AtHorse))
                .ToPropertyEx(this, p => p.AtHorse);
            this.WhenAnyValue(p => p.Cell)
                .Skip(1)
                .Select(cell => cell is AirplaneCell airpane && airpane.IsActive
                                || (cell is LakeCell && AtAirplane))
                .ToPropertyEx(this, p => p.AtAirplane);

            this.WhenAnyValue(p => p.Cell)
                .Skip(1)
                .Where(cell => cell is ArrowCell || cell is CrocodileCell)
                .Subscribe(cell =>
                {
                    if (_loopDict.ContainsKey(cell))
                    {
                        if (++_loopDict[cell] == 4)
                            IsInLoop = true;
                    }
                    else
                        _loopDict[cell] = 1;
                });

            this.WhenAnyValue(p => p.Cell, p => p.Manager.CanUseRum, p => p.Manager.IsRumBlocked, p => p.IsDrunk,
                (cell, canUseRum, isRumBlocked, isDrunk) => cell is ITrapCell && canUseRum && !isRumBlocked && !isDrunk)
                .ToPropertyEx(this, p => p.CanDrinkRum);

            this.WhenAnyValue(p => p.Cell)
                .Skip(1)
                .Select(cell => cell.Number)
                .ToPropertyEx(this, p => p.MazeNodeNumber);

            // присвоение клетки после настройки всех реактивных связей
            Cell = cell;
            Game.Pirates.Add(this);
        }

        /// <summary>
        /// Игрок, владеющий пиратом.
        /// </summary>
        [Reactive] public Player Owner { get; protected set; }
        /// <summary>
        /// Игрок, управляющий пиратом.
        /// </summary>
        [Reactive] public Player Manager { get; set; }
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
        /// Цвет изображения пирата.
        /// </summary>
        public string Image { get; }

        /// <summary>
        /// Клетка, на которой находится пират.
        /// </summary>
        [Reactive] public Cell Cell { get; set; }
        /// <summary>
        /// Клетка, куда движется пират.
        /// </summary>
        public Cell TargetCell { get; set; }
        /// <summary>
        /// Клетка, с которой пират начинает движение.
        /// </summary>
        public Cell StartCell { get; protected set; }
        /// <summary>
        /// Метод для подготовки пирата к движению.
        /// </summary>
        /// <remarks>Метод задаёт <see cref="StartCell"/>, очищает <see cref="_loopDict"/> и удаляет возможности спаивания.</remarks>
        public void PrepareToMove()
        {
            StartCell = Cell;
            _loopDict.Clear();

            CanGiveRumToFriday = false;
            CanGiveRumToMissioner = false;
        }

        /// <summary>
        /// Список координат ячеек, куда пират может пойти.
        /// </summary>
        /// <remarks>
        /// Имеет логику.
        /// </remarks>
        public virtual List<Coordinates> SelectableCoords
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
        [ObservableAsProperty] public int MazeNodeNumber { get; }

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
        [ObservableAsProperty] public virtual bool CanGrabGold { get; }
        /// <summary>
        /// Флаг того, что пират может взять Галеон.
        /// </summary>
        [ObservableAsProperty] public virtual bool CanGrabGaleon { get; }

        /// <summary>
        /// Флаг того, что пират может сражаться и управлять кораблём.
        /// </summary>
        public bool IsFighter { get; }

        /// <summary>
        /// Флаг того, что пират может выпить ром.
        /// </summary>
        [ObservableAsProperty] public virtual bool CanDrinkRum { get; }
        /// <summary>
        /// Флаг того, что пират выпил ром.
        /// </summary>
        [Reactive] public bool IsDrunk { get; set; }
        /// <summary>
        /// Флаг того, что пират может споить Пятницу.
        /// </summary>
        [Reactive] public bool CanGiveRumToFriday { get; set; }
        /// <summary>
        /// Флаг того, что пират может споить Миссионера.
        /// </summary>
        [Reactive] public bool CanGiveRumToMissioner { get; set; }
        /// <summary>
        /// Метод определяет параметры <see cref="CanGiveRumToFriday"/> и <see cref="CanGiveRumToMissioner"/>.
        /// </summary>
        /// <param name="map">Карта игры.</param>
        public void DefineDrinkingOpportynities(Map map)
        {
            CanGiveRumToFriday = false;
            CanGiveRumToMissioner = false;

            if (this is Friday || this is Missioner || !Manager.CanUseRum || Manager.IsRumBlocked)
                return;

            var cells = map.Cells(this).Concat(new Cell[] { Cell });
            foreach (Cell cell in cells.Where(cell => cell is not JungleCell))
            {
                CanGiveRumToFriday |= cell.Pirates.Any(p => p is Friday);
                CanGiveRumToMissioner |= cell.Pirates.Any(p => p is Missioner);
            }
        }

        /// <summary>
        /// Флаг того, что пират может родить пирата.
        /// </summary>
        public virtual bool CanHaveSex => !Manager.IsEnoughtPirates && Cell is FortressCell fortress && fortress.Putana;
        /// <summary>
        /// Метод рождает нового пирата рядом с данным пиратом.
        /// </summary>
        public void GiveBirth() => Cell.AddPirate(new Pirate(Cell, Owner, Manager));

        /// <summary>
        /// Флаг того, что пират заблокирован в яме, пещере или из-за бочки рома.
        /// </summary>
        /// <remarks>Используется в интерфейсе.</remarks>
        [Reactive] public bool IsBlocked { get; set; }
        /// <summary>
        /// Счётчик ходов, которые осталось пропустить из-за бочки с ромом.
        /// </summary>
        public int RumCount { get; set; }

        /// <summary>
        /// Метод удаляет пирата с клетки, на которой он находится.
        /// </summary>
        /// <param name="withGold">Флаг указывает, может ли пират унести золото.</param>
        public void RemoveFromCell(bool withGold = true)
        {
            if (IsDrunk)
                IsDrunk = false;
            Cell.RemovePirate(this, withGold);
        }

        /// <summary>
        /// Метод убивает пирата.
        /// </summary>
        public void Kill()
        {
            Game.OnStartPirateAnimation(this, Cell, 0, kill: true);
            Game.Pirates.Remove(this);

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
            else if(Cell.Galeon > 0)
            {
                Game.LostGold += 3;
                Cell.Galeon = 0;
            }
        }
    }
}
