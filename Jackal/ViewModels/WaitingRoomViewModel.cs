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
    public class WaitingRoomViewModel : ViewModelBase
    {
        public WaitingRoomViewModel()
        {
            Players = new ObservableCollection<Player>();
            IObservable<bool> canStartServer = Players.ToObservableChangeSet()
                                                      .AutoRefresh(player => player.IsReady)
                                                      .ToCollection()
                                                      .Select(p => CheckPlayers(p));

            StartServerCommand = ReactiveCommand.Create(() => Views.MessageBox.Show("Сервер запущен"), canStartServer);
        }
        public ObservableCollection<Player> Players { get; }

        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
        public void AddPlayer() => Players.Add(new Player("Джон Псина", Team.White));
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
