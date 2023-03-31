using Jackal.Models.Cells;
using Jackal.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class GameViewModel : ViewModelBase
    {
        int N = 13;
        Cell[] _cells;

        public GameViewModel()
        {
            _cells = new Cell[N * N];
            for (int i = 0; i < _cells.Length; i++)
            {
                _cells[i] = new Cell(i/N, i%N, "Field");
            }
        }

        public Cell[] Cells => _cells;

        public void func(object param)
        {
            MessageBox.Show("!");
        }
        public bool Canfunc(object param)
        {
            return true;
        }
        public void funcP(object param)
        {
            MessageBox.Show("?");
        }
    }
}
