using Avalonia.Controls.Mixins;
using DynamicData;
using DynamicData.Binding;
using Jackal.Models;
using Jackal.Models.Cells;
using Jackal.Views;
using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
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

            Game.CreateMap();
            _disCells = Game.Map.ToObservableChangeSet()
                                  .Bind(out _cells)
                                  .Subscribe();
        }

        public ReadOnlyObservableCollection<Cell> Cells => _cells;
        public ReadOnlyObservableCollection<Cell> _cells;

        public void func(object param)
        {
            Cell cell = Game.Map[5,5];
            Game.Map[5,5] = Game.Map[0,0];
            Game.Map[0,0] = cell;
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
