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
using Jackal.Network;

namespace Jackal.ViewModels
{
    public class WaitingRoomViewModel : ViewModelBase
    {
        public WaitingRoomViewModel(bool isServerHolder)
        {
            Players = new ObservableCollection<PlayerAdderViewModel>();
            IObservable<bool> canStartServer = Players.ToObservableChangeSet()
                                                      .AutoRefresh(playerVM => playerVM.Player.IsReady)
                                                      .Transform(playersVM => playersVM.Player)
                                                      .ToCollection()
                                                      .Select(player => CheckPlayers(player));

            StartServerCommand = ReactiveCommand.Create(() => Views.MessageBox.Show("Сервер запущен"), canStartServer);

            if (isServerHolder)
                Server.Start();
            Client.Start(Server.IP, this);
        }
        public ObservableCollection<PlayerAdderViewModel> Players { get; }

        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }
        public void AddPlayer(Player player)
        {
            Players.Add(new PlayerAdderViewModel(player));
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
