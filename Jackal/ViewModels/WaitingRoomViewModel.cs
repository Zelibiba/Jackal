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
using Jackal.Views;

namespace Jackal.ViewModels
{
    public class WaitingRoomViewModel : ViewModelBase
    {
        public WaitingRoomViewModel(string? ip = null)
        {
            Players = new ObservableCollection<PlayerAdderViewModel>();

            IObservable<bool> canStartServer = Players.ToObservableChangeSet()
                                                      .AutoRefresh(playerVM => playerVM.Player.IsReady)
                                                      .Transform(playersVM => playersVM.Player)
                                                      .ToCollection()
                                                      .Select(players => CheckPlayers(players));
            StartGameCommand = ReactiveCommand.Create(StartGame, canStartServer);

            IObservable<bool> canChangeWatcher = Players.ToObservableChangeSet()
                                                        .AutoRefresh(playerVM => playerVM.Player.CanChangeWatcher)
                                                        .Transform(playersVM => playersVM.Player)
                                                        .ToCollection()
                                                        .Select(players => players.Count > 1 && players.First().CanChangeWatcher
                                                                           || !players.First().IsControllable);
            ChangeWatcherCommand = ReactiveCommand.Create<bool>(ChangeWatcher, canChangeWatcher);


            IsServerHolder = Server.IsServerHolder;
            IP = ip ?? Server.IP;
        }
        public ObservableCollection<PlayerAdderViewModel> Players { get; }
        public bool IsServerHolder { get; }
        public string IP { get; }


        public ReactiveCommand<Unit, Unit> StartGameCommand { get; }
        public ReactiveCommand<bool, Unit> ChangeWatcherCommand { get; }
        public void ChangeWatcher(bool isWatcher)
        {
            if (isWatcher)
            {
                Client.DeletePlayer();
                Players.RemoveAt(0);
            }
            else
                Client.GetPlayer();
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
        public void AddPlayer(Player player, int index = -1)
        {
            if (index == -1)
                Players.Add(new PlayerAdderViewModel(player));
            else 
                Players.Insert(index, new PlayerAdderViewModel(player));
        }
        public void UpdatePlayer(Player player)
        {
            Players.First(playerVM => playerVM.Player.Index == player.Index).Player.Copy(player);
        }
        public void DeletePlayer(int index)
        {
            Players.Remove(Players.First(playerVM => playerVM.Player.Index == index));
        }

        void StartGame()
        {
            Random rand = new ();
            List<Player> players = Players.Select(vm => vm.Player).ToList();
            Player[] mixedPlayers = new Player[players.Count];
            for (int i = 0; i < mixedPlayers.Length; i++)
            {
                int index = players.FindIndex(player => player.Team == Team.White);
                if (index < 0)
                    index = rand.Next(players.Count);
                mixedPlayers[i] = players[index];
                players.RemoveAt(index);
            }
            int seed = rand.Next();

            Client.StartGame(mixedPlayers, seed);
        }
    }
}
