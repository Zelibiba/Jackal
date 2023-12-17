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
            Index = CellVM.GetIndex();

            this.WhenAnyValue(vm => vm.Index, vm => vm.CellVM.X,
                              (i, x) => x + i % 3 * (Size + 1) + 4)
                              .ToPropertyEx(this, vm => vm.X);
            this.WhenAnyValue(vm => vm.Index, vm => vm.CellVM.Y,
                              (i, y) => y + i / 3 * (Size + 1) + 11)
                              .ToPropertyEx(this, vm => vm.Y);
        }

        public int Size => 18;
        public Pirate Pirate { get; }
        [Reactive] public CellViewModel CellVM { get; private set; }
        public void SetCell(CellViewModel cellVM)
        {
            CellVM.ClearIndex(Index);
            CellVM = cellVM;
            Index = CellVM.GetIndex();
        }

        [Reactive] public int Index { get; private set; }

        [ObservableAsProperty] public int X { get; }
        [ObservableAsProperty] public int Y { get; }
    }
}
