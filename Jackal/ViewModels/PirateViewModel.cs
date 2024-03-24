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
            CellVM = cellsVM.First(cellVM => cellVM.Cell.Coords == pirate.Cell.Coords);
            Index = CellVM.GetIndex();

            this.WhenAnyValue(vm => vm.Index, vm => vm.CellVM.X,
                              (i, x) => x + i % 3 * (_size + 1) + 4)
                              .ToPropertyEx(this, vm => vm.X);
            this.WhenAnyValue(vm => vm.Index, vm => vm.CellVM.Y,
                              (i, y) => y + i / 3 * (_size + 1) + 16)
                              .ToPropertyEx(this, vm => vm.Y);

            this.WhenAnyValue(vm => vm.Pirate.Manager.Turn, vm => vm.Pirate.Manager.IsControllable, vm => vm.Pirate.IsEnabled,
                             (turn, isControllable, isEnabled) => turn && isControllable && isEnabled)
                             .ToPropertyEx(this, vm => vm.CanBeSelected);
        }

        static readonly int _size = 18;
        public Pirate Pirate { get; }
        [Reactive] CellViewModel CellVM { get; set; }
        public void SetCell(CellViewModel cellVM)
        {
            if (CellVM == cellVM) return;

            CellVM.ClearIndex(Index);
            CellVM = cellVM;
            Index = CellVM.GetIndex();
        }
        public void Kill()
        {
            CellVM.ClearIndex(Index);
        }

        [Reactive] int Index { get; set; }

        [ObservableAsProperty] public int X { get; }
        [ObservableAsProperty] public int Y { get; }

        /// <summary>
        /// Флаг того, что с пиратом можно взаимодействовать.
        /// </summary>
        /// <remarks>Необходим для анимации.</remarks>
        [ObservableAsProperty] public bool CanBeSelected { get; }
    }
}
