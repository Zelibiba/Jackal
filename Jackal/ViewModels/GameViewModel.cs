using Avalonia.Controls.Mixins;
using DynamicData;
using DynamicData.Binding;
using Jackal.Models;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
using Jackal.Views;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.ViewModels
{
    public class GameViewModel : ViewModelBase, IActivatableViewModel
    {
        public ViewModelActivator Activator { get; }
        IDisposable _disCells;

        public GameViewModel()
        {
            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                _disCells.DisposeWith(disposable);
            });

            this.WhenAnyValue(vm => vm.SelectedCell)
                .Where(cell => cell != null && (cell.CanBeSelected || cell is ShipCell))
                .Subscribe(cell => SelectCell(cell));
            this.WhenAnyValue(vm => vm.SelectedPirate)
                .Where(pirate => pirate != null)
                .Subscribe(pirate => SelectPirate(pirate));

            if (!Game.CreateMap())
                return;
            _disCells = Game.Map.ToObservableChangeSet()
                                .Bind(out _cells)
                                .Subscribe();
        }


        public ReadOnlyObservableCollection<Cell> Cells => _cells;
        public ReadOnlyObservableCollection<Cell> _cells;
        [Reactive] public Cell SelectedCell { get; set; }
        [Reactive] public Pirate SelectedPirate { get; set; }


        void SelectCell(Cell cell)
        {
            Game.SelectCell(cell);
            //SelectedCell = null;
        }
        void SelectPirate(Pirate pirate)
        {
            Game.SelectPirate(pirate);
            //SelectedPirate = null;
        }
        public void Deselect()
        {
            Game.Deselect();
            SelectedPirate = null;
            SelectedCell = null;
        }
    }
}
