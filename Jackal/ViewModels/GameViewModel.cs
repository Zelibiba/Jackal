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
        int N = 13 * 13;
        Cell[] _cells;

        public GameViewModel()
        {
            _cells = new Cell[N];
            for(int i=0;i<N;i++)
            {
                _cells[i] = new Cell();
            }
        }

        public Cell[] Cells => _cells;

        public void func(object param)
        {
            MessageBox.Show("!");
        }
    }
}
