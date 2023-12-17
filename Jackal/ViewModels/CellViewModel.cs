using Jackal.Models;
using Jackal.Models.Cells;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class CellViewModel : ViewModelBase
    {
        static CellViewModel()
        {
            Height = 64;
            Width = Map.Type == MapType.Quadratic ? 64 : 56;
            

            if (Map.Type == MapType.Quadratic)
            {
                ComputeX = (row, column) => Width * column;
                ComputeY = (row) => Height * row;
            }
            else
            {
                ComputeX = (row, column) => Width * column + (row - 7) * (Width / 2);
                ComputeY = (row) => Height * 3 / 4 * row;
            }
        }
        public CellViewModel(Cell cell)
        {
            Cell = cell;
            _piratePlaces = new();

            this.WhenAnyValue(vm => vm.Cell.Row, vm => vm.Cell.Column, ComputeX)
                .ToPropertyEx(this, vm => vm.X);
            this.WhenAnyValue(vm => vm.Cell.Row, ComputeY)
                .ToPropertyEx(this, vm => vm.Y);
        }

        public Cell Cell { get; }

        
        public static int Height { get; }
        public static int Width { get; }

        readonly static Func<int, int, int> ComputeX;
        readonly static Func<int, int> ComputeY;
        [ObservableAsProperty] public int X { get; }
        [ObservableAsProperty] public int Y { get; }

        private readonly List<bool> _piratePlaces;
        public void ClearIndex(int index)
        {
            _piratePlaces[index] = false;
        }
        public int GetIndex()
        {
            int index = _piratePlaces.FindIndex(i => !i);
            if (index < 0)
            {
                index = _piratePlaces.Count;
                _piratePlaces.Add(true);
            }
            else
                _piratePlaces[index] = true;
            return index;
        }
    }
}
