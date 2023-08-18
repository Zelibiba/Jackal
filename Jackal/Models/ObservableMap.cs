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
    public class ObservableMap : ObservableCollection<Cell>
    {
        /// <summary>
        /// <inheritdoc cref="ObservableMap" path="/summary"/>
        /// </summary>
        /// <param name="mapSize">Размер квадратного массива.</param>
        public ObservableMap(int mapSize, int seed)
        {
            MapSize = mapSize;
            Seed = seed;
        }

        public readonly int MapSize;
        public readonly int Seed;
        public Cell this[int row, int column]
        {
            get
            {
                if (CheckIndexes(row, column))
                    throw new ArgumentOutOfRangeException("Wrong indexes of map!");
                return this[row * MapSize + column];
            }
            set
            {
                if (CheckIndexes(row, column))
                    throw new ArgumentOutOfRangeException("Wrong indexes of map!");
                this[row * MapSize + column] = value;
            }
        }
        public Cell this[int[] coords]
        {
            get
            {
                if (coords.Length != 2)
                    throw new ArgumentException("Wrong massive of coordinates!");
                return this[coords[0], coords[1]];
            }
            set
            {
                if (coords.Length != 2)
                    throw new ArgumentException("Wrong massive of coordinates!");
                this[coords[0], coords[1]] = value;
            }
        }
        /// <summary>
        /// Метод возвращает перечисление клеток, имеющих заданные координаты.
        /// </summary>
        /// <param name="coordinates">Список координат клеток.</param>
        /// <returns></returns>
        public IEnumerable<Cell> Cells(IEnumerable<int[]> coordinates) => coordinates.Select(coords => this[coords]);
        /// <summary>
        /// Метод возвращает перечисление клеток, достижимых пиратом.
        /// </summary>
        /// <param name="pirate"></param>
        /// <returns></returns>
        public IEnumerable<Cell> Cells(Pirate pirate) => pirate.SelectableCoords.Select(coords => this[coords].GetSelectedCell(pirate));

        /// <summary>
        /// Метод проверяет, не вышли ли индексы за границы карты.
        /// </summary>
        /// <param name="row">Номер строки.</param>
        /// <param name="column">Номер столбца.</param>
        /// <param name="mapSize">Размер карты.</param>
        /// <returns>False, если проверка пройдена.</returns>
        public static bool CheckIndexes(int row, int column, int mapSize)
        {
            return row < 0 || column < 0 || row >= mapSize || column >= mapSize;
        }
        /// <summary>
        /// <inheritdoc cref="CheckIndexes(int, int, int)" path="/summary"/>
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        /// <returns><inheritdoc cref="CheckIndexes(int, int, int)" path="/returns"/></returns>
        public bool CheckIndexes(int row, int column) => CheckIndexes(row, column, MapSize); 
    }
}
