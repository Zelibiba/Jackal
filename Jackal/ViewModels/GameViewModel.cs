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
        //static bool _falshStart = true;


        public GameViewModel(string? filename = null, IEnumerable<Player>? players = null, int seed = -1, bool isEnabled = true)
        {
            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                _disCells.DisposeWith(disposable);
                _disPlayers.DisposeWith(disposable);
            });

            //if (_falshStart)
            //{
            //    _falshStart = false;
            //    return;
            //}

            if (filename == null)
            {
                if (players != null)
                    Game.CreateMap(players, seed);
                else
                    Game.CreateMap(new Player[]{new Player(0, "TEST", Team.White, true),
                                                new Player(1, "DD", Team.Black, true),
                                                new Player(2, "AETHNAETRN", Team.Red, true),
                                                new Player(3, "djk", Team.Yellow, true) });
            }
            else
                FileHandler.ReadSave(filename);
            Game.EnableInterface = (isEnabled) => IsEnabled = isEnabled;
            Game.DeselectPirate = () => SelectedPirate = Pirate.Empty;
            Game.SetWinner = (player) => Views.MessageBox.Show("Ура победителю: " + player.Name + "!");
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
            IsEnabled = isEnabled;
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
