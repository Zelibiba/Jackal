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
        /// <returns>Копия клетки.</returns>
        public static Cell Copy(Cell cell)
        {
            return new Cell(cell.Row, cell.Column, cell.Image, angle: cell.Angle)
            {
                Pirates = cell.Pirates,
                IsOpened = cell.IsOpened,
                IsVisible = cell.IsVisible
            };
        }

        /// <summary>
        /// Название изображения клетки.
        /// </summary>
        [Reactive] public string Image { get; private set; }
        /// <summary>
        /// Угол, на который повёрнута клетка.
        /// </summary>
        public virtual int Angle { get; }
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
        /// Метод возвращает ту часть клетки, на которую может перейти данный пират.
        /// </summary>
        /// <remarks>
        /// В обычном случае возвращает саму клетку. Возвращает иное в случаях лабиринта, пещеры.
        /// </remarks>
        public virtual Cell GetSelectedCell(Pirate pirate) => this;


        /// <summary>
        /// Флаг того, что на клетке может стоять пират.
        /// </summary>
        public readonly bool IsStandable;
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
        /// Имеет логику.
        /// </remarks>
        public bool IsOpened
        {
            get => __isOpened;
            protected set
            {
                __isOpened = value;
                IsPreOpened = value;
            }
        }
        bool __isOpened;
        /// <summary>
        /// Метод открывает клетку.
        /// </summary>
        public virtual void Open() => IsOpened = true;
        /// <summary>
        /// Флаг того, что клетка может быть выбрана.
        /// </summary>
        [Reactive] public bool CanBeSelected { get; set; }
        /// <summary>
        /// Флаг того, что клетка выбрана во время землетрясения.
        /// </summary>
        [Reactive] public bool IsSelected { get; set; }

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
        /// Метод определяет, может ли пират зайти на данную клетку с сокровищем.
        /// </summary>
        /// <param name="pirate">Пират, заходящий на клетку.</param>
        /// <returns>true, если пират может зайти с сокровищем.</returns>
        public virtual bool IsGoldFriendly(Pirate pirate) => IsOpened;

        /// <summary>
        /// Список пиратов, находящихся на клетке.
        /// </summary>
        public ObservableCollection<Pirate> Pirates { get; protected set; }
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
        public virtual void RemovePirate(Pirate pirate)
        {
            Pirates.Remove(pirate);

            if (pirate.Gold)
                Gold--;
            else if (pirate.Galeon)
                Galeon = false;
        }
         /// <summary>
        /// Метод помещает пирата на клетку.
        /// </summary>
        /// <param name="pirate">Пират, добавляемый на клетку.</param>
        /// <returns></returns>
        public virtual MovementResult AddPirate(Pirate pirate)
        {
            Pirates.Add(pirate);
            pirate.Cell = this;

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
