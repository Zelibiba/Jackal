using Avalonia.Media.Imaging;
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
        /// <param name="isStandable">Флаг того, что пират может стоять на клетке.</param>
        public Cell(int row, int column, string image, bool isStandable = true, int angle = 0)
        {
            Row = row;
            Column= column;
            Image = image;
            Angle = angle;
            Pirates = new ObservableCollection<Pirate>();
            SelectableCoords = new List<int[]>();

            this.WhenAnyValue(c => c.Gold, c => c.Galeon)
                .Select(t => t.Item1 > 0 || t.Item2)
                .ToPropertyEx(this, c => c.Treasure);

            IsVisible = true;
            IsStandable = isStandable;
            Nodes = new ObservableCollection<Cell>() { this };
            Number = 0;
        }
        /// <summary>
        /// Метод создаёт поверхностную копию клетки.
        /// </summary>
        /// <param name="cell">Клетка, с которой копируются значения.</param>
        /// <remarks>Необходим для анимации перемещения ячейки.</remarks>
        public static Cell Copy(Cell cell)
        {
            return new Cell(cell.Row, cell.Column, cell.Image, angle: cell.Angle)
            {
                Pirates = cell.Pirates,
                IsPreOpened = cell.IsPreOpened,
                IsVisible = cell.IsVisible
            };
        }

        /// <summary>
        /// Название изображения клетки.
        /// </summary>
        [Reactive] public string Image { get; protected set; }
        /// <summary>
        /// Угол, на который повёрнута клетка.
        /// </summary>
        public virtual int Angle { get; }
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
        public int Row { get; protected set; }
        /// <summary>
        /// Столбец клетки.
        /// </summary>
        public int Column { get; protected set; }
        /// <summary>
        /// Координаты клетки.
        /// </summary>
        public int[] Coords => new int[] { Row, Column };
        /// <summary>
        /// Флаг того, что данная клетка имеет такие же координаты.
        /// </summary>
        /// <param name="row">Строка.</param>
        /// <param name="column">Колонка.</param>
        /// <returns>true, если координаты такие же.</returns>
        public bool HasSameCoords(int row, int column) => row == Row && column == Column;
        /// <summary>
        /// <inheritdoc cref="HasSameCoords(int, int)" path="/summary"/>
        /// </summary>
        /// <param name="coords">Сравниваемые координаты.</param>
        /// <returns><inheritdoc cref="HasSameCoords(int, int)" path="/returns"/></returns>
        public bool HasSameCoords(int[] coords) => coords.Length == 2 && HasSameCoords(coords[0], coords[1]);
        /// <summary>
        /// Метод задаёт новые координаты клетки.
        /// </summary>
        /// <param name="row">Новая строка.</param>
        /// <param name="column">Новый столбец.</param>
        public virtual void SetCoordinates(int row, int column)
        {
            Row = row;
            Column = column;
        }

        /// <summary>
        /// Список координат клеток, на которые можно переместиться с данной клетки.
        /// </summary>
        public List<int[]> SelectableCoords { get; }
        /// <summary>
        /// Метод определяет координаты клеток, куда пират может походить.
        /// </summary>
        /// <param name="map">Карта игры.</param>
        public virtual void SetSelectableCoords(ObservableMap map)
        {
            SelectableCoords.Clear();
            for (int i = -1; i < 2; i++)
            {
                for (int j = -1; j < 2; j++)
                {
                    if ((i == 0 && j == 0) || map[Row + i, Column + j] is SeaCell)
                        continue;
                    SelectableCoords.Add(new int[2] { Row + i, Column + j });
                }
            }
        }

        /// <summary>
        /// Список уровней лабиринта.
        /// </summary>
        /// <remarks>
        /// В нелабиринтной клетке имеет единственный элемент, ссылающийся на саму клетку.
        /// </remarks>
        public ObservableCollection<Cell> Nodes { get; }
        /// <summary>
        /// Номер уровня лабиринта данной клетки.
        /// </summary>
        /// <remarks>
        /// В нелабиринтной клетке равняется 0.
        /// </remarks>
        public int Number { get; protected set; }
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
        public readonly bool IsStandable;

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
            if (pirate.Cell.Pirates.Count == 1 && (pirate is Friday || pirate is Missioner)
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
        public ObservableCollection<Pirate> Pirates { get; protected set; }
        /// <summary>
        /// Команда, которая владеет данной клеткой.
        /// </summary>
        protected virtual Team Team => Pirates.ElementAtOrDefault(0)?.Team ?? Team.None;
        /// <summary>
        /// Флаг того, что на клетке не стоят вражеские пираты.
        /// </summary>
        /// <param name="pirate">Выбранный пират.</param>
        /// <returns>True, если на клетке есть враги.</returns>
        public bool IsFriendlyTo(Pirate pirate) => (pirate.Alliance | Team.None).HasFlag(Team);

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
                    Gold--;
                else if (pirate.Galeon)
                    Galeon = false;
            }
        }
         /// <summary>
        /// Метод помещает пирата на клетку.
        /// </summary>
        /// <param name="pirate">Пират, добавляемый на клетку.</param>
        /// <returns></returns>
        public virtual MovementResult AddPirate(Pirate pirate)
        {
            Pirates.Add(pirate);    // Сначала добавить пирата, потом убрать врагов (для корректной работы AirplaneCell)
            pirate.Cell = this;

            // Побить пиратов на клетке и забрать пятницу (или умереть самому)
            if (!IsFriendlyTo(pirate))
            {
                if(!pirate.IsFighter)
                {
                    pirate.Kill();
                    return MovementResult.End;
                }

                List<Pirate> pirates = new();
                foreach (Pirate pir in Pirates.Take(Pirates.Count - 1))
                {
                    if (pir is Friday friday)
                        friday.SetNewOwner(pirate.Owner);
                    else
                        pirates.Add(pir);
                }
                foreach (Pirate pir in pirates)
                {
                    pir.Gold = false;
                    pir.Galeon = false;
                    pir.TargetCell = pir.Owner.Ship;
                    RemovePirate(pir);
                    pir.Owner.Ship.AddPirate(pir);
                }
            }

            if (pirate.Gold)
                Gold++;
            else if (pirate.Galeon)
                Galeon = true;

            if (!IsOpened)
                Open();

            return IsStandable ? MovementResult.End : MovementResult.Continue;
        }
    }
}
