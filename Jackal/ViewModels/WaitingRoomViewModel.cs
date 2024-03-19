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
using Avalonia;
using System.IO;

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
                                                        .AutoRefresh(playerVM => playerVM.Player.IsReady)
                                                        .Transform(playersVM => playersVM.Player)
                                                        .ToCollection()
                                                        .Select(players => players.Count > 1 && !players.First().IsReady
                                                                           || !players.First().IsControllable);
            ChangeWatcherCommand = ReactiveCommand.Create<bool>(ChangeWatcher, canChangeWatcher);

            IObservable<bool> canCreateAlly = Players.ToObservableChangeSet()
                                                     .AutoRefresh(playerVM => playerVM.Player.IsReady)
                                                     .Transform(playersVM => playersVM.Player)
                                                     .ToCollection()
                                                     .Select(players => players.First().IsControllable && !players.First().IsReady);
            CreateAllyCommand = ReactiveCommand.Create<bool>(CreateAlly, canCreateAlly);

            this.WhenAnyValue(vm => vm.IsHexagonal)
                .Skip(1)
                .Subscribe(x => {
                    GameProperties.MapType = x ? MapType.Hexagonal : MapType.Quadratic;
                    GameProperties.NormaliseSize();
                });
            this.WhenAnyValue(vm => vm.GameProperties.PatternName)
                .Skip(1)
                .Subscribe(x => {
                    IsFixed = x == "Фиксированный";
                    if (IsFixed) GameProperties.NormaliseSize();
                    else Client.ChangeGameProperties(GameProperties);
                });
            this.WhenAnyValue(vm => vm.GameProperties.Size)
                .Skip(1)
                .Subscribe(x => Client.ChangeGameProperties(GameProperties));

            GameProperties = new();
            IP = ip ?? Server.IP;
        }
        public ObservableCollection<PlayerAdderViewModel> Players { get; }
        public bool IsServerHolder => Server.IsServerHolder;
        public string IP { get; }

        [Reactive] public GameProperties GameProperties { get; private set; }
        /// <summary>
        /// Флаг того, что тип карты - гексагональный.
        /// </summary>
        [Reactive] public bool IsHexagonal { get; set; }
        [Reactive] public bool IsFixed { get; private set; }


        public ReactiveCommand<Unit, Unit> StartGameCommand { get; }
        void StartGame()
        {
            if (!GameProperties.ReadMapPattern())
            {
                MessageBox.Show("Невозможно прочесть файл паттерна карты!");
                return;
            }

            Random rand = new();
            List<Player> players = Players.Select(vm => vm.Player).ToList();
            Player[] mixedPlayers = new Player[players.Count];

            int index = players.FindIndex(p => p.Team == Team.White);
            if (index < 0)
                index = rand.Next(players.Count);
            mixedPlayers[0] = players[index];
            players.RemoveAt(index);

            int I = 0;
            for (int i = 1; i < mixedPlayers.Length; i++)
            {
                if (mixedPlayers[0].AllianceIdentifier == AllianceIdentifier.None)
                {
                    index = rand.Next(players.Count);
                    mixedPlayers[i] = players[index];
                    players.RemoveAt(index);
                }
                else
                {
                    Player? player = players.FirstOrDefault(p => !mixedPlayers[..i].Any(pl => pl.AllianceIdentifier == p.AllianceIdentifier));
                    if (player != null)
                    {
                        List<Player> alliedPlayers = players.FindAll(p => p.AllianceIdentifier == player.AllianceIdentifier);
                        player = alliedPlayers[rand.Next(alliedPlayers.Count)];
                        mixedPlayers[i] = player;
                        players.Remove(player);
                    }
                    else
                    {
                        if (I == 0) I = i;
                        List<Player> alliedPlayers = players.FindAll(p => p.AllianceIdentifier == mixedPlayers[i - I].AllianceIdentifier);
                        player = alliedPlayers[rand.Next(alliedPlayers.Count)];
                        mixedPlayers[i] = player;
                        players.Remove(player);
                    }
                }
            }
            GameProperties.Seed = rand.Next();

            Client.StartGame(mixedPlayers, GameProperties);
        }

        public ReactiveCommand<bool, Unit> ChangeWatcherCommand { get; }
        void ChangeWatcher(bool isWatcher)
        {
            if (isWatcher)
            {
                Client.DeletePlayer(Players[0].Player.Index);
                Players.RemoveAt(0);
            }
            else
                Client.GetPlayer();
        }
        public ReactiveCommand<bool,Unit> CreateAllyCommand { get; }
        void CreateAlly(bool create)
        {
            if (create)
                Client.GetPlayer();
            else
            {
                Client.DeletePlayer(Players[1].Player.Index);
                Players.RemoveAt(1);
            }
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
                Players.Insert(index, new PlayerAdderViewModel(player, index == 0));
        }
        public void UpdatePlayer(Player player)
        {
            Players.First(playerVM => playerVM.Player.Index == player.Index).Player.Copy(player);
        }
        public void DeletePlayer(int index)
        {
            Players.Remove(Players.First(playerVM => playerVM.Player.Index == index));
        }
        public void ChangeGameProperties(GameProperties properties)
        {
            GameProperties = properties;
        }
    }
}
