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
        public static readonly Pirate Empty = new() { IsVisible = true };
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
            Image = image ?? Team.ToString();
            IsVisible = true;

            IsFighter = isFighter;

            this.WhenAnyValue(p => p.Gold)
                .Skip(1)
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
            _mazeNodeNumber = this.WhenAnyValue(p => p.Cell)
                                  .Skip(1)
                                  .Select(cell => cell.Number)
                                  .ToProperty(this, p => p.MazeNodeNumber);
            this.WhenAnyValue(p => p.Cell, p => p.Manager.IsEnoughtPirates)
                .Select(x => x.Item1 is FortressCell fortress && fortress.Putana && !x.Item2)
                .ToPropertyEx(this, p => p.CanHaveSex);
            this.WhenAnyValue(p => p.MazeNodeNumber)
                .Select(number => number > 0 && number < (Cell?.Number ?? 0))
                .ToPropertyEx(this, p => p.CanDrinkRum);
        }

        /// <summary>
        /// Игрок, владеющий пиратом.
        /// </summary>
        public Player Owner { get; protected set; }
        /// <summary>
        /// Игрок, управляющий пиратом.
        /// </summary>
        public Player Manager { get; protected set; }
        /// <summary>
        /// Команда пирата.
        /// </summary>
        public Team Team => Owner?.Team ?? Team.None;
        /// <summary>
        /// Альянс, в котором состоит пират.
        /// </summary>
        public Team Alliance => Owner.Alliance;

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
        /// Номер уровня лабиринта, на котором находится пират.
        /// </summary>
        public int MazeNodeNumber => _mazeNodeNumber?.Value ?? 0;
        readonly ObservableAsPropertyHelper<int> _mazeNodeNumber;
        /// <summary>
        /// Клетка, с которой пират начинает движение.
        /// </summary>
        public Cell StartCell { get; protected set; }
        /// <summary>
        /// Метод задаёт <see cref="StartCell"/> как клетку, где сейчас находится пират.
        /// </summary>
        public void Set_StartCell() => StartCell = Cell;
        /// <summary>
        /// Клетка, куда движется пират.
        /// </summary>
        public Cell TargetCell;

        /// <summary>
        /// Список координат ячеек, куда пират может пойти.
        /// </summary>
        public List<int[]> SelectableCoords => Cell.SelectableCoords;

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
        /// Флаг того, что пират может сражаться и управлять кораблём.
        /// </summary>
        readonly public bool IsFighter;
        /// <summary>
        /// Флаг того, что пират может родить пирата.
        /// </summary>
        [ObservableAsProperty] public virtual bool CanHaveSex { get; }
        /// <summary>
        /// Флаг того, что пират может выпить ром.
        /// </summary>
        [ObservableAsProperty] public bool CanDrinkRum { get; }


        public bool IsBlocked => false;

        /// <summary>
        /// Метод удаляет пирата с клетки, на которой он находится.
        /// </summary>
        public void RemoveFromCell() => Cell.RemovePirate(this);

        /// <summary>
        /// Метод убивает пирата.
        /// </summary>
        public void Kill()
        {
            Cell.Pirates.Remove(this);
            Manager.Pirates.Remove(this);
        }
        /// <summary>
        /// Метод рождает нового пирата рядом с данным пиратом.
        /// </summary>
        public void GiveBirth()
        {
            Pirate newPirate = new(Owner, Manager);
            Cell.AddPirate(newPirate);
            Manager.Pirates.Add(newPirate);
        }
    }
}
