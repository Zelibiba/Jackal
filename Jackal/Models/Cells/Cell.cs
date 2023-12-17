using Avalonia.Media.Imaging;
using DynamicData;
using DynamicData.Binding;
using Jackal.Models.Pirates;
using Microsoft.CodeAnalysis;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models.Cells
{
    /// <summary>
    /// Базовый класс клетки.
    /// </summary>
    public class Cell : ReactiveObject
    {
        /// <summary>
        /// Конструктор клетки.
        /// </summary>
        /// <param name="row">Строка клетки.</param>
        /// <param name="column">Колонка клетки.</param>
        /// <param name="image">Название изображения клетки.</param>
        /// <param name="isStandable"><inheritdoc cref="IsStandable" path="/summary"/></param>
        /// <param name="angle"><inheritdoc cref="Angle" path="/summary"/></param>
        /// <param name="number"><inheritdoc cref="Number" path="/summary"/></param>
        public Cell(int row, int column, string image, bool isStandable = true, int number = 0)
        {
            Row = row;
            Column = column;
            Coords = new(Row, Column);
            Image = image;
            Angle = 0;
            Pirates = new List<Pirate>();
            SelectableCoords = new List<Coordinates>();
            Nodes = new ObservableCollection<Cell> { this };
            IsVisible = true;
            IsStandable = isStandable;
            Number = number;

            this.WhenAnyValue(c => c.Gold, c => c.Galeon,
                (gold, galeon) => gold > 0 || galeon)
                .ToPropertyEx(this, c => c.Treasure);
        }

        /// <summary>
        /// Угол, на который повёрнута клетка.
        /// </summary>
        public virtual int Angle { get; }
        /// <summary>
        /// Название изображения клетки.
        /// </summary>
        [Reactive] public string Image { get; private set; }
        /// <summary>
        /// Флаг того, что изображение чёрно-белое.
        /// </summary>
        bool IsGray => Image.EndsWith("_gray");
        /// <summary>
        /// Метод смены тона изображения на чёрно-белый и обратно.
        /// </summary>
        public void ChangeGrayStatus()
        {
            if (IsGray)
                Image = Image[0..^5];
            else
                Image += "_gray";
        }

        /// <summary>
        /// Строка клетки.
        /// </summary>
        public int Row { get; private set; }
        /// <summary>
        /// Столбец клетки.
        /// </summary>
        public int Column { get; private set; }
        /// <summary>
        /// Координаты клетки.
        /// </summary>
        public Coordinates Coords {get; private set; }
        /// <summary>
        /// Метод задаёт новые координаты клетки.
        /// </summary>
        /// <param name="row">Новая строка.</param>
        /// <param name="column">Новый столбец.</param>
        public virtual void SetCoordinates(int row, int column)
        {
            Row = row;
            Column = column;
            Coords = new(Row, Column);
            this.RaisePropertyChanged(nameof(Row));
            this.RaisePropertyChanged(nameof(Column));
        }

        /// <summary>
        /// Список координат клеток, на которые можно переместиться с данной клетки.
        /// </summary>
        public List<Coordinates> SelectableCoords { get; }
        /// <summary>
        /// Метод определяет координаты клеток, куда пират может походить.
        /// </summary>
        /// <param name="map">Карта игры.</param>
        public virtual void SetSelectableCoords(Map map)
        {
            SelectableCoords.Clear();
            foreach(Coordinates coords in map.AdjacentCellsCoords(this))
            {
                if (map[coords] is SeaCell) continue;
                SelectableCoords.Add(coords);
            }
        }

        /// <summary>
        /// Список уровней лабиринта.
        /// </summary>
        public ObservableCollection<Cell> Nodes { get; }
        /// <summary>
        /// Номер уровня лабиринта данной клетки.
        /// </summary>
        /// <remarks>
        /// В нелабиринтной клетке равняется 0.
        /// </remarks>
        public int Number { get; }

        /// <summary>
        /// Метод возвращает ту часть клетки, которая досягаема для выбранного пирата.
        /// </summary>
        /// <remarks>
        /// В обычном случае возвращает саму клетку. Возвращает иное в случаях лабиринта, пещеры.
        /// </remarks>
        public virtual Cell GetSelectedCell(Pirate pirate) => this;

        /// <summary>
        /// Флаг того, что клетка видима.
        /// </summary>
        /// <remarks>
        /// Необходим для анимации перемещения клеток.
        /// </remarks>
        [Reactive] public bool IsVisible { get; set; }
        /// <summary>
        /// Флаг того, что изображение клетки видимо.
        /// </summary>
        [Reactive] public bool IsPreOpened { get; set; }
        /// <summary>
        /// Флаг того, что клетка открыта пиратом.
        /// </summary>
        /// <remarks>
        /// Изменять параметр только через <see cref="Open"/>
        /// </remarks>
        public bool IsOpened { get; protected set; }
        /// <summary>
        /// Метод открывает клетку.
        /// </summary>
        public virtual void Open()
        {
            if (IsGray)
                ChangeGrayStatus();
            IsOpened = true;
            IsPreOpened = true;
        }

        /// <summary>
        /// Флаг того, что на клетке может стоять пират.
        /// </summary>
        public bool IsStandable { get; }

        /// <summary>
        /// Флаг того, что клетка может быть выбрана.
        /// </summary>
        [Reactive] public bool CanBeSelected { get; set; }
        /// <summary>
        /// Флаг того, что клетка выбрана во время землетрясения или хода маяка.
        /// </summary>
        [Reactive] public bool IsSelected { get; set; }
        /// <summary>
        /// Флаг того, что клетка была вскрыта во время хода маяка.
        /// </summary>
        /// <remarks>
        /// Необходима для окрашивания фона клетки.
        /// </remarks>
        [Reactive] public bool IsLightHousePicked { get; set; }
        /// <summary>
        /// Метод определения возможности перемещения пирата без сокровищ.
        /// </summary>
        /// <param name="pirate"></param>
        /// <returns>True, если можно переместиться.</returns>
        public virtual bool CanBeSelectedBy(Pirate pirate)
        {
            if ((pirate is Friday || pirate is Missioner)
                && Pirates.Count == 1 && (Pirates[0] is Friday || Pirates[0] is Missioner))
                return true;

            return IsFriendlyTo(pirate) || pirate.IsFighter && !ContainsMissioner && !pirate.Cell.ContainsMissioner;
        }
        /// <summary>
        /// Метод определения занчения <see cref="CanBeSelected"/>.
        /// </summary>
        /// <param name="pirate">Выбранный пират.</param>
        public void DefineSelectability(Pirate pirate)
        {
            if (pirate.Treasure)
                CanBeSelected = IsGoldFriendly(pirate);
            else
                CanBeSelected = CanBeSelectedBy(pirate);
        }


        /// <summary>
        /// Количество монет на клетке.
        /// </summary>
        [Reactive] public virtual int Gold { get; set; }
        /// <summary>
        /// Флаг того, что на клетке находится Галеон.
        /// </summary>
        [Reactive] public virtual bool Galeon { get; set; }
        /// <summary>
        /// Флаг того, что на клетке находится сокровище.
        /// </summary>
        [ObservableAsProperty] public bool Treasure { get; }
        /// <summary>
        /// Метод определения возможности перемещения пирата с сокровищем.
        /// </summary>
        /// <param name="pirate">Пират, заходящий на клетку.</param>
        /// <returns>true, если пират может зайти с сокровищем.</returns>
        public virtual bool IsGoldFriendly(Pirate pirate) => IsOpened && IsFriendlyTo(pirate);

        /// <summary>
        /// Список пиратов, находящихся на клетке.
        /// </summary>
        public List<Pirate> Pirates { get; protected set; }
        /// <summary>
        /// Команда, которая владеет данной клеткой.
        /// </summary>
        protected virtual Team Team => Pirates.ElementAtOrDefault(0)?.Team ?? Team.None;
        /// <summary>
        /// Флаг того, что на клетке не стоят вражеские пираты.
        /// </summary>
        /// <param name="pirate">Выбранный пират.</param>
        /// <returns>True, если на клетке есть враги.</returns>
        public virtual bool IsFriendlyTo(Pirate pirate) => (pirate.Alliance | Team.None).HasFlag(Team);

        /// <summary>
        /// Флаг того, что на клетке находится Миссионер.
        /// </summary>
        public bool ContainsMissioner => Pirates.Any(pirate => pirate is Missioner);
        /// <summary>
        /// Флаг того, что на клетке находится Пятница.
        /// </summary>
        public bool ContainsFriday => Pirates.Any(pirate => pirate is Friday);

        /// <summary>
        /// Флаг того, что клетка является кораблём.
        /// </summary>
        /// <remarks>
        /// Необходим для интерфейса.
        /// </remarks>
        public virtual bool IsShip => false;
        /// <summary>
        /// Команда, которой принадлежит корабль.
        /// </summary>
        /// <remarks>
        /// Необходим для интерфейса. Имеет смысл, только если клетка является кораблём.
        /// </remarks>
        public virtual Team ShipTeam => Team.None;


        /// <summary>
        /// Метод убирает пирата с клетки.
        /// </summary>
        /// <param name="pirate">Пират, убираемый с клетки.</param>
        /// <param name="withGold">Флаг указывает, учитывать ли уносимое пиратом золото.</param>
        public virtual void RemovePirate(Pirate pirate, bool withGold = true)
        {
            Pirates.Remove(pirate);

            if (withGold)
            {
                if (pirate.Gold)
                {
                    Gold--;
                    if (Gold == 0)
                    {
                        foreach (Pirate p in Pirates)
                            p.Gold = false;
                    }
                }
                else if (pirate.Galeon)
                {
                    Galeon = false;
                    foreach (Pirate p in Pirates)
                        p.Galeon = false;
                }
            }
            else
            {
                pirate.Gold = false;
                pirate.Galeon = false;
            }
        }
         /// <summary>
        /// Метод помещает пирата на клетку.
        /// </summary>
        /// <param name="pirate">Пират, добавляемый на клетку.</param>
        /// <returns></returns>
        public virtual MovementResult AddPirate(Pirate pirate, int delay = 0)
        {
            Game.OnStartPirateAnimation(pirate, this);
            if (delay > 0)
                Task.Delay(delay).Wait();

            Pirates.Add(pirate);    // Сначала добавить пирата, потом убрать врагов (для корректной работы AirplaneCell)
            pirate.Cell = this;

            // Побить пиратов на клетке и забрать пятницу
            HitPirates(pirate);

            if (pirate.Gold)
                Gold++;
            else if (pirate.Galeon)
                Galeon = true;

            if (!IsOpened)
                Open();

            return IsStandable ? MovementResult.End : MovementResult.Continue;
        }
        /// <summary>
        /// Метод стукает вражеских пиратов на клетке и забирает пятницу, если это необходимо делать.
        /// </summary>
        /// <param name="pirate">Пират, который пришёл на данную клетку.</param>
        public void HitPirates(Pirate pirate, bool allPirates = false)
        {
            if (!IsFriendlyTo(pirate))
            {
                if(pirate is Friday && ContainsMissioner || pirate is Missioner && ContainsFriday)
                {
                    while (Pirates.Count > 0)
                        Pirates[0].Kill();
                    return;
                }

                List<Pirate> pirates = new();
                foreach (Pirate pir in Pirates.Take(Pirates.Count - (allPirates ? 0 : 1)))
                {
                    if (pir is Friday friday)
                        friday.SetNewOwner(pirate.Owner, pirate.Manager);
                    else
                        pirates.Add(pir);
                }
                foreach (Pirate pir in pirates)
                {
                    pir.TargetCell = pir.Owner.Ship;
                    RemovePirate(pir, withGold: false);
                    pir.Owner.Ship.AddPirate(pir, delay: 150);
                }
            }
        }
    }
}
