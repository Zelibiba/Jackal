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
        readonly IDisposable _disPlayers;
        readonly IDisposable _disPirates;


        public GameViewModel(IEnumerable<Player>? players, int seed, IEnumerable<int[]>? operations = null)
        {
            Activator = new ViewModelActivator();
            this.WhenActivated(disposable =>
            {
                _disPlayers.DisposeWith(disposable);
            });

            Game.CreateMap(players, seed, autosave: operations == null);
            Game.ReadOperations(operations ?? new List<int[]>());
            Game.StartPirateAnimation = (cellIndex) =>
            {
                PirateViewModel pirate = Pirates.First(vm => vm.Pirate == Game.SelectedPirate);
                Cells[cellIndex].AddPirate(pirate);
                return Task.Delay(300);
            };
            Game.EnableInterface = (isEnabled) => IsEnabled = isEnabled;
            Game.DeselectPirate = () => SelectedPirate = Pirate.Empty;
            Game.SetWinner = (players) => Views.MessageBox.Show(string.Format("Ура победител{0}:\n{1} !",
                                                                              players.Count() > 1 ? "ям" : "ю",
                                                                              string.Join('\n', players.Select(p => p.Name))));
            SelectedPirate = Pirate.Empty;

            Cells = (from cell in Game.Map
                     select new CellViewModel(cell)).ToArray();

            _disPlayers = Game.Players.ToObservableChangeSet()
                                      .Bind(out _players)
                                      .Subscribe();
            _disPirates = Game.Pirates.ToObservableChangeSet()
                                      .Transform(p => new PirateViewModel(p, Cells))
                                      .Bind(out _pirates)
                                      .Subscribe();

            this.WhenAnyValue(vm => vm.SelectedPirate)
                .Select(pirate => pirate != Pirate.Empty)
                .ToPropertyEx(this, vm => vm.IsPirateSelected);

            HiddenGold = Game.HiddenGold;
            IsEnabled = players.First().IsControllable;
        }

        public int MapHeight => Map.Type == MapType.Quadratic ? Map.RowsCount * CellViewModel.Height :
                                                                CellViewModel.Height + (Map.RowsCount - 1) * CellViewModel.Height * 3 / 4;
        public int MapWidth => CellViewModel.Width * Map.ColumnsCount;

        [Reactive] public bool IsEnabled { get; set; }

        public CellViewModel[] Cells { get; }

        public ReadOnlyObservableCollection<Player> Players => _players;
        readonly ReadOnlyObservableCollection<Player> _players;
        public ReadOnlyObservableCollection<PirateViewModel> Pirates => _pirates;
        readonly ReadOnlyObservableCollection<PirateViewModel> _pirates;

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
