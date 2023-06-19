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
using Avalonia.Threading;
using System.Numerics;
using System.Runtime.CompilerServices;

namespace Jackal.ViewModels
{
    public class WaitingRoomViewModel : ViewModelBase
    {
        public WaitingRoomViewModel(Action<ViewModelBase> setViewModel, string? ip = null)
        {
            Players = new ObservableCollection<PlayerAdderViewModel>();
            IObservable<bool> canStartServer = Players.ToObservableChangeSet()
                                                      .AutoRefresh(playerVM => playerVM.Player.IsReady)
                                                      .Transform(playersVM => playersVM.Player)
                                                      .ToCollection()
                                                      .Select(players => CheckPlayers(players));

            StartServerCommand = ReactiveCommand.Create(MixPlayers, canStartServer);

            SetViewModel = setViewModel;

            IsServerHolder = ip == null;
            if (IsServerHolder)
            {
                Server.Start();
                IP = Server.IP;
            }
            else
                IP = ip;
            Client.Start(IP, this);
        }
        public ObservableCollection<PlayerAdderViewModel> Players { get; }
        public bool IsServerHolder { get; }
        public string IP { get; }

        readonly Action<ViewModelBase> SetViewModel;

        public ReactiveCommand<Unit, Unit> StartServerCommand { get; }


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
        public void AddPlayer(Player player) => Players.Add(new PlayerAdderViewModel(player));
        public void UpdatePlayer(Player player)
        {
            foreach (PlayerAdderViewModel playerVM in Players)
                if (playerVM.Player.Index == player.Index)
                {
                    playerVM.Player.Copy(player);
                    break;
                }
        }
        public void DeletePlasyer(int index)
        {
            foreach (PlayerAdderViewModel playerVM in Players)
                if (playerVM.Player.Index == index)
                {
                    Players.Remove(playerVM);
                    break;
                }
        }

        void MixPlayers()
        {
            Random rand = new Random();
            List<Team> teams = Players.Select(vm => vm.Player.Team).ToList();
            Team[] mixedTeams = new Team[teams.Count];
            for (int i = 0; i < mixedTeams.Length; i++)
            {
                int index = teams.IndexOf(Team.White);
                index = (index >= 0) ? index : rand.Next(teams.Count);
                mixedTeams[i] = teams[index];
                teams.RemoveAt(index);
            }
            int seed = rand.Next();

            Client.StartGame(mixedTeams, seed);
            Dispatcher.UIThread.InvokeAsync(() => StartGame(mixedTeams, seed));
        }
        public void StartGame(Team[] mixedTeam, int mapSeed)
        {
            Player[] players = new Player[Players.Count];
            for (int i = 0; i < players.Length; i++)
                players[i] = Players.First(vm => vm.Player.Team == mixedTeam[i]).Player;

            bool isEnabled = Players[0].Player.Team == players[0].Team;

            SetViewModel(new GameViewModel(players: players, seed: mapSeed, isEnabled: isEnabled));
        }
    }
}
