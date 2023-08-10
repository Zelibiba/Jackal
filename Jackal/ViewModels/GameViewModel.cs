using Avalonia.Animation;
using Avalonia.Controls.Mixins;
using Avalonia.Threading;
using DynamicData;
using DynamicData.Binding;
using Jackal.Models;
using Jackal.Models.Cells;
using Jackal.Models.Pirates;
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


        public GameViewModel(IEnumerable<Player>? players, int seed, IEnumerable<int[]>? operations = null)
        {
            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                _disCells.DisposeWith(disposable);
                _disPlayers.DisposeWith(disposable);
            });

            Game.CreateMap(players, seed, autosave: operations == null);
            foreach (int[] operation in operations ?? new List<int[]>())
            {
                switch ((Actions)operation[0])
                {
                    case Actions.MovePirate:
                        int index = operation[1];
                        bool gold = operation[2] == 1;
                        bool galeon = operation[3] == 1;
                        int[] coords = operation[4..];
                        Game.SelectPirate(index, gold, galeon, coords);
                        Game.SelectCell(coords); break;
                    case Actions.MoveShip:
                        Game.SelectCell(Game.CurrentPlayer.ManagedShip);
                        Game.SelectCell(operation[1..]); break;
                    case Actions.CellSelection:
                        Game.SelectCell(operation[1..]); break;
                    case Actions.DrinkRum:
                        index = operation[1];
                        ResidentType type = (ResidentType)operation[2];
                        Game.SelectPirate(index);
                        Game.GetDrunk(type); break;
                    case Actions.GetBirth:
                        index = operation[1];
                        Game.SelectPirate(index);
                        Game.PirateBirth(); break;
                }
            }
            Game.EnableInterface = (isEnabled) => IsEnabled = isEnabled;
            Game.DeselectPirate = () => SelectedPirate = Pirate.Empty;
            Game.SetWinner = (players) => Views.MessageBox.Show(string.Format("Ура победител{0}:\n{1} !",
                                                                              players.Count() > 1 ? "ям" : "ю",
                                                                              string.Join('\n', players.Select(p => p.Name))));
            SelectedPirate = Pirate.Empty;

            _disCells = Game.Map.ToObservableChangeSet()
                                .Bind(out _cells)
                                .Subscribe();
            _disPlayers = Game.Players.ToObservableChangeSet()
                                      .Bind(out _players)
                                      .Subscribe();
            this.WhenAnyValue(vm => vm.SelectedPirate)
                .Select(pirate => pirate != Pirate.Empty)
                .ToPropertyEx(this, vm => vm.IsPirateSelected);

            HiddenGold = Game.HiddenGold;
            IsEnabled = players.First().IsControllable;
        }

        [Reactive] public bool IsEnabled { get; set; }

        public ReadOnlyObservableCollection<Cell> Cells => _cells;
        readonly ReadOnlyObservableCollection<Cell> _cells;

        public ReadOnlyObservableCollection<Player> Players => _players;
        readonly ReadOnlyObservableCollection<Player> _players;

        [Reactive] public Pirate SelectedPirate { get; set; }
        [ObservableAsProperty] public bool IsPirateSelected { get; }

        [Reactive] public int HiddenGold { get; private set; }
        [Reactive] public int CurrentGold { get; private set; }
        [Reactive] public int LostGold { get; private set; }

        public void SelectCell(Cell cell)
        {
            if (Game.PreSelectCell(cell))
            {
                Task.Run(() =>
                {
                    Game.SelectCell(cell);

                    HiddenGold = Game.HiddenGold;
                    CurrentGold = Game.CurrentGold;
                    LostGold = Game.LostGold;
                });
            }
        }
        public void SelectPirate(Pirate pirate)
        {
            IsEnabled = false;
            if (Game.CanChangeSelection && Game.PreSelectPirate(pirate))
            {
                Game.SelectPirate(pirate);
                SelectedPirate = pirate;
            }
            IsEnabled = true;
        }
        public void Deselect()
        {
            IsEnabled = false;
            if (Game.CanChangeSelection)
                Game.Deselect();
            IsEnabled = true;
        }

        public void GrabTreasure(string param)
        {
            IsEnabled = false;
            SelectedPirate = Pirate.Empty;
            Game.GrabGold(param);
            SelectedPirate = Game.SelectedPirate;
            IsEnabled = true;
        }
        public void GetPirateDrunk(object param)
        {
            IsEnabled = false;
            switch (param)
            {
                case "pirate":
                    Game.GetDrunk(ResidentType.Ben); break;
                case "friday":
                    Game.GetDrunk(ResidentType.Friday); break;
                case "missioner":
                    Game.GetDrunk(ResidentType.Missioner); break;
            }
            IsEnabled = true;
        }
        public void PirateBirth(object param)
        {
            IsEnabled = false;
            Game.PirateBirth();
        }


        public void ShowField(object param) => Game.ShowField();
    }
}
