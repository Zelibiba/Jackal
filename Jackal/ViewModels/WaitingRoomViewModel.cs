using Jacal;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DynamicData;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using Jackal.Models;
using DynamicData.Binding;
using System.Reactive.Disposables;

namespace Jackal.ViewModels
{
    public class WaitingRoomViewModel : ViewModelBase, IActivatableViewModel
    {
        public WaitingRoomViewModel()
        {
            _players = new ObservableCollectionExtended<Player>();
            players = new ReadOnlyObservableCollection<Player>(_players);
            IObservable<bool> canStartServer;

            Activator = new ViewModelActivator();
            this.WhenActivated(disposables =>
            {
                _players.ToObservableChangeSet()
                        .Bind(out players)
                        .Subscribe()
                        .DisposeWith(disposables);
            });
            canStartServer = _players.ToObservableChangeSet()
                                     .AutoRefresh(player => player.IsReady)
                                     .ToCollection()
                                     .Select(p => CheckPlayers(p));

            StartServerCommand = ReactiveCommand.Create(() => Views.MessageBox.Show("Сервер запущен"), canStartServer);
        }
        public ViewModelActivator Activator { get; }

        private ObservableCollectionExtended<Player> _players;
        
        public ReadOnlyObservableCollection<Player> Players => players;
        private ReadOnlyObservableCollection<Player> players;

        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
        public void AddPlayer()
        {
            _players.Add(new Player("Джон Псина", Team.White));
        }
        private bool CheckPlayers(IEnumerable<Player> players)
        {
            if (players.Count() < 2)
                return false;

            Team usedTeams = Team.None;
            foreach (Player player in players)
            {
                if (!player.IsReady || usedTeams.HasFlag(player.Team))
                    return false;
                usedTeams |= player.Team;
            }
            return true;
        }
    }
}
