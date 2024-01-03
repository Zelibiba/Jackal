using Avalonia;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    /// <summary>
    /// Клас карты.
    /// </summary>
    /// <remarks>
    /// Наследуется от ObservableCollection&lt;<see cref="Cell"/>&gt;.
    /// </remarks>
    public class Map : List<Cell>
    {
        /// <summary>
        /// <inheritdoc cref="Map" path="/summary"/>
        /// </summary>
        /// <param name="mapSize">Размер квадратного массива.</param>
        public Map(GameProperties properties)
        {
            Type = properties.MapType;

            if (Type == MapType.Quadratic)
            {
                RowsCount = properties.Size + 2;
                ColumnsCount = properties.Size + 2;
                _indexes = new int?[RowsCount, ColumnsCount];
                for (int i = 0; i < RowsCount; i++)
                {
                    for (int j = 0; j < ColumnsCount; j++)
                        _indexes[i, j] = i * RowsCount + j;
                }
                _adjacentCellsCoordsPattern = new Coordinates[8]
                {
                    new Coordinates(-1,  0),
                    new Coordinates(-1, -1),
                    new Coordinates( 0, +1),
                    new Coordinates( 0, -1),
                    new Coordinates(+1, +1),
                    new Coordinates(+1,  0),
                    new Coordinates(+1, -1),
                    new Coordinates(-1, +1)
                };

                #region Определение характеристик кораблей
                ShipPlacements = new ShipPlacement[4]
                {
                    new(new Coordinates[] { new(+1, 0) }, properties.Size - 2),
                    new(new Coordinates[] { new(0, +1) }, properties.Size - 2),
                    new(new Coordinates[] { new(-1, 0) }, properties.Size - 2),
                    new(new Coordinates[] { new(0, -1) }, properties.Size - 2)
                };
                for (int i = 0; i < properties.Size - 2; i++)
                {
                    ShipPlacements[0].Region[i] = new(0, i + 2);
                    ShipPlacements[1].Region[i] = new(i + 2, 0);
                    ShipPlacements[2].Region[i] = new(properties.Size + 1, i + 2);
                    ShipPlacements[3].Region[i] = new(i + 2, properties.Size + 1);
                }
                foreach (ShipPlacement shipPlacement in ShipPlacements)
                {
                    int aveIndex = shipPlacement.Region.Length / 2;
                    shipPlacement.InitialCoordinates = shipPlacement.Region[aveIndex];
                }
                #endregion
            }
            else
            {
                RowsCount = 2 * properties.Size + 1;
                ColumnsCount = 2 * properties.Size + 1;
                _indexes = new int?[RowsCount, ColumnsCount];
                (int min, int max)[] rowSizes = new (int min, int max)[RowsCount];
                for (int i = 0; i < properties.Size + 1; i++)
                    rowSizes[i] = (properties.Size - i, 2 * properties.Size);
                for (int i = properties.Size + 1; i < RowsCount; i++)
                    rowSizes[i] = (0, 3 * properties.Size - i);

                int I = 0;
                for(int i = 0; i < RowsCount; i++)
                {
                    int j = 0;
                    for (; j < rowSizes[i].min; j++)
                        _indexes[i, j] = null;
                    for (; j <= rowSizes[i].max; j++)
                        _indexes[i, j] = I++;
                    for (; j < ColumnsCount; j++)
                        _indexes[i, j] = null;
                }
                _adjacentCellsCoordsPattern = new Coordinates[6]
                {
                    new Coordinates(-1,  0),
                    new Coordinates( 0, -1),
                    new Coordinates(+1, -1),
                    new Coordinates(+1,  0),
                    new Coordinates( 0, +1),
                    new Coordinates(-1, +1)
                };

                #region Определение характеристик кораблей
                ShipPlacements = new ShipPlacement[6]
                {
                    new(new Coordinates[] { new(+1, 0), new(+1,-1) }, properties.Size - 1),
                    new(new Coordinates[] { new( 0,+1), new(+1, 0) }, properties.Size - 1),
                    new(new Coordinates[] { new(-1,+1), new( 0,+1) }, properties.Size - 1),
                    new(new Coordinates[] { new(-1, 0), new(-1,+1) }, properties.Size - 1),
                    new(new Coordinates[] { new( 0,-1), new(-1, 0) }, properties.Size - 1),
                    new(new Coordinates[] { new(+1,-1), new( 0,-1) }, properties.Size - 1)
                };
                for (int i = 0; i < properties.Size - 1; i++)
                {
                    ShipPlacements[0].Region[i] = new(0, 2 * properties.Size - 1 - i);
                    ShipPlacements[1].Region[i] = new(1 + i, properties.Size - 1 - i);
                    ShipPlacements[2].Region[i] = new(properties.Size + 1 + i, 0);
                    ShipPlacements[3].Region[i] = new(2 * properties.Size, 1 + i);
                    ShipPlacements[4].Region[i] = new(2 * properties.Size - 1 - i, properties.Size + 1 + i);
                    ShipPlacements[5].Region[i] = new(properties.Size - 1 - i, 2 * properties.Size);
                }
                foreach (ShipPlacement shipPlacement in ShipPlacements)
                {
                    int aveIndex = shipPlacement.Region.Length / 2;
                    shipPlacement.InitialCoordinates = shipPlacement.Region[aveIndex];
                }
                #endregion
            }

            for (int row = 0; row < RowsCount; row++)
            {
                for (int column = 0; column < ColumnsCount; column++)
                {
                    if (_indexes[row, column] != null)
                        Add(new SeaCell(row, column));
                }
            }
        }

        /// <summary>
        /// Тип карты.
        /// </summary>
        public static MapType Type { get; private set; } = MapType.Quadratic;
        /// <summary>
        /// Количество строк.
        /// </summary>
        /// <remarks>Необходимо для определения высоты контрола карты.</remarks>
        public static int RowsCount { get; private set; }
        /// <summary>
        /// Количество столбцов.
        /// </summary>
        /// <remarks><inheritdoc cref="RowsCount" path="/remarks"/></remarks>
        public static int ColumnsCount { get; private set; }

        /// <summary>
        /// Массив соответствия двойного и оригинального индексов.
        /// </summary>
        readonly int?[,] _indexes;
        /// <summary>
        /// Паттерн относительных координат соседних клеток.
        /// </summary>
        readonly Coordinates[] _adjacentCellsCoordsPattern;
        /// <summary>
        /// Массив характеристик всех возможных кораблей на карте.
        /// </summary>
        public ShipPlacement[] ShipPlacements { get; }


        /// <summary>
        /// Метод возвращает координаты соседних клеток.
        /// </summary>
        /// <param name="cell">Клетка, соседи которых рассматриваются.</param>
        /// <param name="pattern">Перечисление относительных координат соседей.</param>
        /// <returns></returns>
        public IEnumerable<Coordinates> AdjacentCellsCoords(Cell cell, IEnumerable<Coordinates>? pattern = null)
        {
            return (pattern ?? _adjacentCellsCoordsPattern).Select(coords => cell.Coords + coords)
                                                           .Where(coords => coords.Row >= 0 && coords.Column >= 0
                                                                         && coords.Row < RowsCount && coords.Column < ColumnsCount
                                                                         && _indexes[coords.Row, coords.Column] != null);
        }

        /// <summary>
        /// Возвращает лист координат всех земляных клеток карты.
        /// </summary>
        /// <returns></returns>
        public List<Coordinates> GroundCoordinates()
        {
            List<Coordinates> result = new();
            for (int row = 1; row < RowsCount - 1; row++)
            {
                int min = 0;
                int max = 0;
                for (int column = 0; column < ColumnsCount; column++)
                {
                    if (_indexes[row, column] != null)
                    {
                        min = column;
                        break;
                    }
                }
                for (int column = ColumnsCount - 1; column >= 0; column--)
                {
                    if (_indexes[row, column] != null)
                    {
                        max = column;
                        break;
                    }
                }

                for (int column = min + 1; column < max; column++)
                {
                    if (Type == MapType.Quadratic &&
                        (row == 1 || row == RowsCount - 2) && (column == min + 1 || column == max - 1))
                        continue;
                    result.Add(new(row, column));
                }
            }
            return result;
        }
        /// <summary>
        /// Метод меняет местами индексы для обращения к клетке при их перемене местами.
        /// </summary>
        /// <param name="coord1">Координаты первой ячейки.</param>
        /// <param name="coord2">Координаты второй ячейки.</param>
        public void SwapIndexes(Coordinates coord1, Coordinates coord2)
        {
            int? index1 = _indexes[coord1.Row, coord1.Column];
            _indexes[coord1.Row, coord1.Column] = _indexes[coord2.Row, coord2.Column];
            _indexes[coord2.Row, coord2.Column] = index1;
        }

        public Cell this[int row, int column]
        {
            get => this[(int)_indexes[row, column]];
            set
            {
                this[(int)_indexes[row, column]] = value;
            }
        }
        public Cell this[Coordinates coords]
        {
            get => this[coords.Row, coords.Column];
            set
            {
                this[coords.Row, coords.Column] = value;
            }
        }
        public new int IndexOf(Cell cell) => (int)_indexes[cell.Row, cell.Column];

        /// <summary>
        /// Метод возвращает перечисление клеток, имеющих заданные координаты.
        /// </summary>
        /// <param name="coordinates">Список координат клеток.</param>
        /// <returns></returns>
        public IEnumerable<Cell> Cells(IEnumerable<Coordinates> coordinates) => coordinates.Select(coords => this[coords]);
        /// <summary>
        /// Метод возвращает перечисление клеток, достижимых пиратом.
        /// </summary>
        /// <param name="pirate"></param>
        /// <returns></returns>
        public IEnumerable<Cell> Cells(Pirate pirate) => pirate.SelectableCoords.Select(coords => this[coords].GetSelectedCell(pirate));

        /// <summary>
        /// Метод создаёт корабль для игрока.
        /// </summary>
        /// <param name="shipIndex">Расположение создаваемого корабля.</param>
        /// <param name="player"></param>
        public void SetShipToPlayer(int shipIndex, Player player)
        {
            ShipPlacement shipPlacement = ShipPlacements[shipIndex];
            this[shipPlacement.InitialCoordinates] = new ShipCell(shipPlacement, player);
        }
    }
}
