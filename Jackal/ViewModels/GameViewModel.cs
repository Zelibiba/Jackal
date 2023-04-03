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


            if (!Game.CreateMap())
                return;
            _disCells = Game.Map.ToObservableChangeSet()
                                .Bind(out _cells)
                                .Subscribe();
        }


        public ReadOnlyObservableCollection<Cell> Cells => _cells;
        ReadOnlyObservableCollection<Cell> _cells;

        [Reactive] public Pirate SelectedPirate { get; set; }

        public void SelectCell(Cell cell) => Game.PreSelectCell(cell);
        public void SelectPirate(Pirate pirate) => Game.PreSelectPirate(pirate);
        public void Deselect() => Game.Deselect();
    }
}
