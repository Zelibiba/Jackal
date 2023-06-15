using Jackal.Models.Cells;
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
        public ObservableMap(int mapSize)
        {
            MapSize = mapSize;
        }

        public readonly int MapSize;
        public Cell this[int row, int column]
        {
            get
            {
                CheckIndexes(row, column);
                return this[row * MapSize + column];
            }
            set
            {
                CheckIndexes(row, column);
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
        /// Метод проверяет, не вышли ли индексы за границы массива.
        /// </summary>
        /// <param name="row"></param>
        /// <param name="column"></param>
        void CheckIndexes(int row, int column)
        {
            if (row < 0 || column < 0 || row >= MapSize || column >= MapSize)
                throw new ArgumentOutOfRangeException("Wrong indexes of map!");
        }
    }
}
