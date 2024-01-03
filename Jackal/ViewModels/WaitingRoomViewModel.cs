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
                                                        .AutoRefresh(playerVM => playerVM.Player.CanChangeWatcher)
                                                        .Transform(playersVM => playersVM.Player)
                                                        .ToCollection()
                                                        .Select(players => players.Count > 1 && players.First().CanChangeWatcher
                                                                           || !players.First().IsControllable);
            ChangeWatcherCommand = ReactiveCommand.Create<bool>(ChangeWatcher, canChangeWatcher);

            IObservable<bool> canCreateAlly = Players.ToObservableChangeSet()
                                                     .AutoRefresh(playerVM => playerVM.Player.CanChangeWatcher)
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
                .Subscribe(x => {
                    IsFixed = x == "Фиксированный";
                    if (IsFixed) GameProperties.NormaliseSize();
                    else Client.ChangeGameProperties(GameProperties);
                });
            this.WhenAnyValue(vm => vm.GameProperties.Size)
                .Subscribe(x => Client.ChangeGameProperties(GameProperties));

            GameProperties = new();
            IsServerHolder = Server.IsServerHolder;
            IP = ip ?? Server.IP;
        }
        public ObservableCollection<PlayerAdderViewModel> Players { get; }
        public bool IsServerHolder { get; }
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
            if (!ReadMapPattern())
                return;

            Random rand = new();
            List<Player> players = Players.Select(vm => vm.Player).ToList();
            Player[] mixedPlayers = new Player[players.Count];
            int I = 0;
            if (players.Count == 4)
            {
                mixedPlayers[0] = players.Find(player => player.Team == Team.White);
                players.Remove(mixedPlayers[0]);
                I = 1;
                if (mixedPlayers[0].AllianceIdentifier != AllianceIdentifier.None)
                {
                    int index = rand.Next(players.Count);
                    while (players[index].AllianceIdentifier == mixedPlayers[0].AllianceIdentifier)
                        index = rand.Next(players.Count);
                    mixedPlayers[1] = players[index];
                    players.RemoveAt(index);
                    mixedPlayers[2] = players.Find(player => player.AllianceIdentifier == mixedPlayers[0].AllianceIdentifier);
                    players.Remove(mixedPlayers[2]);
                    mixedPlayers[3] = players[0];
                    I = 4;
                }
            }
            for (; I < mixedPlayers.Length; I++)
            {
                int index = players.FindIndex(player => player.Team == Team.White);
                if (index < 0)
                    index = rand.Next(players.Count);
                mixedPlayers[I] = players[index];
                players.RemoveAt(index);
            }
            GameProperties.Seed = rand.Next();

            Client.StartGame(mixedPlayers, GameProperties);
        }
        bool ReadMapPattern()
        {
            string filename = GameProperties.PatternNames[GameProperties.PatternName];
            bool mainFix = filename == "fix";
            if (filename == "fix" || filename == "var")
                filename = GameProperties.MapType.ToString();
            try
            {
                filename = Path.Combine(Properties.MapPatternsFolder, filename + ".txt");
                FileStream? file = new(filename, FileMode.Open, FileAccess.Read);
                StreamReader reader = new(file);

                Dictionary<string, (int, char)> mapPattern = new();
                while (!reader.EndOfStream)
                {
                    string[] words = reader.ReadLine().Split(':');
                    words[1] = words[1].Trim();
                    char param = ' ';
                    if (mainFix)
                        param = '=';
                    else if (words[1][0] < '0' || words[1][0] > '9')
                    {
                        param = words[1][0];
                        words[1] = words[1].Remove(0, 1);
                    }
                    int count = int.Parse(words[1]);
                    mapPattern[words[0]] = (count, param);
                }
                GameProperties.MapPattern = mapPattern;
            }
            catch { return false; }
            return true;
        }
        public ReactiveCommand<bool, Unit> ChangeWatcherCommand { get; }
        void ChangeWatcher(bool isWatcher)
        {
            if (isWatcher)
            {
                Client.DeletePlayer(0);
                Players.RemoveAt(0);
            }
            else
                Client.GetPlayer(0);
        }
        public ReactiveCommand<bool,Unit> CreateAllyCommand { get; }
        void CreateAlly(bool create)
        {
            if (create)
                Client.GetPlayer(1);
            else
            {
                Client.DeletePlayer(1);
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
                Players.Insert(index, new PlayerAdderViewModel(player, index != 0));
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
