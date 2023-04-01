using Jackal.Models.Cells;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Models
{
    public class ObservableMap : ObservableCollection<Cell>
    {
        public ObservableMap(int mapSize)
        {
            _mapSize = mapSize;
        }

        readonly int _mapSize;
        public Cell this[int row, int column]
        {
            get
            {
                CheckIndexes(row, column);
                return this[row * _mapSize + column];
            }
            set
            {
                CheckIndexes(row, column);
                this[row * _mapSize + column] = value;
            }
        }

        void CheckIndexes(int row, int column)
        {
            if (row < 0 || column < 0 || row >= _mapSize || column >= _mapSize)
                throw new ArgumentOutOfRangeException("Map");
        }
    }
}
