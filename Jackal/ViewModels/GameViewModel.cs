﻿using Avalonia.Animation;
using Avalonia.Controls.Mixins;
using Avalonia.Threading;
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
        readonly IDisposable _disCells;
        readonly IDisposable _disPlayers;
        static bool _falshStart = true;


        public GameViewModel()
        {
            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                _disCells.DisposeWith(disposable);
                _disPlayers.DisposeWith(disposable);
            });

            if (_falshStart)
            {
                _falshStart = false;
                return;
            }

            IsEnabled = true;
            Game.CreateMap();
            Game.DeselectInVM = () => SelectedPirate = Pirate.Empty;
            Game.SetIsEnable = (isEnabled) => IsEnabled = isEnabled;
            SelectedPirate = Pirate.Empty;

            _disCells = Game.Map.ToObservableChangeSet()
                                .Bind(out _cells)
                                .Subscribe();
            _disPlayers = Game.Players.ToObservableChangeSet()
                                      .Bind(out _players)
                                      .Subscribe();
            _isPirateSelected = this.WhenAnyValue(vm => vm.SelectedPirate)
                                    .Select(pirate => pirate != Pirate.Empty)
                                    .ToProperty(this, vm => vm.IsPirateSelected);
        }

        [Reactive] public bool IsEnabled { get; set; }

        public ReadOnlyObservableCollection<Cell> Cells => _cells;
        readonly ReadOnlyObservableCollection<Cell> _cells;

        public ReadOnlyObservableCollection<Player> Players => _players;
        readonly ReadOnlyObservableCollection<Player> _players;

        [Reactive] public Pirate SelectedPirate { get; set; }
        public bool IsPirateSelected => _isPirateSelected.Value;
        readonly ObservableAsPropertyHelper<bool> _isPirateSelected;

        public void SelectCell(Cell cell) => Game.PreSelectCell(cell);
        public void SelectPirate(Pirate pirate)
        {
            if (Game.CanChangeSelection)
            {
                Game.SelectPirate(pirate, true);
                SelectedPirate = pirate;
            }
        }
        public void Deselect()
        {
            if (Game.CanChangeSelection)
                Game.Deselect();
        }

        public void GrabTreasure(string param) => Game.ReselctPirate(param);
        public void PirateBirth(object param) => Game.PirateBirth();
    }
}
