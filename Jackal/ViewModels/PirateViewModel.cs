using Jackal.Models.Pirates;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class PirateViewModel : ViewModelBase
    {
        public PirateViewModel(Pirate pirate, CellViewModel[] cellsVM)
        {
            Pirate = pirate;
            CellVM = cellsVM.First(cellVM => cellVM.Cell.ShipTeam == pirate.Team);
            _index = CellVM.PiratesVM.Count;
            CellVM.PiratesVM.Add(this);

            this.WhenAnyValue(vm => vm.Index, vm => vm.CellVM.X, vm => vm.CellVM.Y,
                (i, x, y) => x + i % 3 * (Size + 1) + 4)
                .ToPropertyEx(this, vm => vm.X);
            this.WhenAnyValue(vm => vm.Index, vm => vm.CellVM.X, vm => vm.CellVM.Y,
                (i, x, y) => y + i / 3 * (Size + 1) + 11)
                .ToPropertyEx(this, vm => vm.Y);
        }

        public Pirate Pirate { get; }
        [Reactive] public CellViewModel CellVM { get; private set; }
        public void SetCell(CellViewModel cellVM, int index)
        {
            CellVM.PiratesVM[Index] = null;
            CellVM = cellVM;

            Index = index;
        }

        int Index
        {
            get => _index;
            set
            {
                _index = value;
                this.RaisePropertyChanged(nameof(Index));
            }
        }
        int _index;
        [ObservableAsProperty] public int X { get; }
        [ObservableAsProperty] public int Y { get; }

        [Reactive] public TimeSpan AnimationDuration { get; private set; }
        static readonly TimeSpan _pirateDuration = TimeSpan.FromSeconds(0.3);

        public int Size => 18;
    }
}
