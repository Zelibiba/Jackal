using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Tmds.DBus;
using Jackal.Models;
using Avalonia.Threading;
using Jackal.ViewModels;
using System.Threading;
using System.Reflection;
using Jackal.Models.Pirates;
using Jackal.Models.Cells;
using System.Reflection.PortableExecutable;

namespace Jackal.Network
{
    public static class Client
    {
        static string _ip;
        static TcpClient _client;
        static NetLogStream _stream;
        static BinaryLogReader _reader;
        static BinaryLogWriter _writer;

        static Task _listening;
        static CancellationTokenSource _cancellationTokenSource;
        static NetMode _lastMode;
        static bool _blockAction;

        public static Action<ViewModelBase> _SetContent;
        static WaitingRoomViewModel _viewModel;
        

        public static void Start(string ip, Action<ViewModelBase> SetContent)
        {
            try
            {
                _ip = ip;
                _client = new TcpClient();
                _client.Connect(ip, 10001);
                _stream = new(_client.GetStream(), "client.log");
                _reader = new BinaryLogReader(_stream);
                _writer = new BinaryLogWriter(_stream);
                _cancellationTokenSource = new CancellationTokenSource();
                _SetContent = SetContent;

                if (Properties.Version != _reader.ReadString('\n'))
                    throw new Exception("Версия клиента не соответсвтвует версии сервера!");

                _stream.WriteLog("Prepare to game:");
                bool preapreToGame = _reader.ReadBoolean('\n');
                if (preapreToGame)
                {
                    _viewModel = new WaitingRoomViewModel(_ip);
                    _SetContent(_viewModel);
                    _viewModel.AddPlayer(_reader.ReadPlayer(isControllable: true));
                    int playerCount = _reader.ReadInt32();
                    for (int i = 0; i < playerCount; i++)
                        _viewModel.AddPlayer(_reader.ReadPlayer());
                    _viewModel.ChangeGameProperties(_reader.ReadGameProperties());
                }
                else
                {
                    int count = _reader.ReadInt32();
                    Player[] players = new Player[count];
                    for (int i = 0; i < count; i++)
                        players[i] = _reader.ReadPlayer();
                    GameProperties properties = _reader.ReadGameProperties();

                    count = _reader.ReadInt32();
                    int[][] operations = new int[count][];
                    for (int i = 0; i < count; i++)
                    {
                        int length = _reader.ReadInt32();
                        operations[i] = new int[length];
                        for (int j = 0; j < length; j++)
                            operations[i][j] = _reader.ReadInt32();
                    }

                    _blockAction = true;
                    _SetContent(new GameViewModel(players, properties, operations));
                    _blockAction = false;
                }

                _listening = Task.Run(ReceiveMessages);
            }
            catch (Exception ex)
            {
                Dispatcher.UIThread.Post(() => Views.MessageBox.Show("Client.Start: " + ex.Message));
                Close();
            }
        }
        static async Task ReceiveMessages()
        {
            try
            {
                bool continueListening = true;
                byte[] buffer = new byte[1];
                while (continueListening)
                {
                    _ = await _stream.ReadAsync(buffer.AsMemory(0, 1), _cancellationTokenSource.Token);
                    _lastMode = _reader.ReadNetMode();
                    _blockAction = true;
                    switch (_lastMode)
                    {
                        case NetMode.Disconnect:
                            Dispatcher.UIThread.Post(() => Views.MessageBox.Show("Сервер разорвал соединение"));
                            await Task.Delay(500);
                            continueListening = false;
                            break;
                        case NetMode.NewPlayer:
                            RunInUIThread(() => _viewModel.AddPlayer(_reader.ReadPlayer()));
                            break;
                        case NetMode.GetPlayer:
                            int number = _reader.ReadInt32();
                            RunInUIThread(() => _viewModel.AddPlayer(_reader.ReadPlayer(isControllable: true), number));
                            break;
                        case NetMode.UpdatePlayer:
                            RunInUIThread(() => _viewModel.UpdatePlayer(_reader.ReadPlayer()));
                            break;
                        case NetMode.DeletePlayer:
                            RunInUIThread(() => _viewModel?.DeletePlayer(_reader.ReadInt32('\n')));
                            break;
                        case NetMode.ChangeGameProperties:
                            RunInUIThread(() => _viewModel?.ChangeGameProperties(_reader.ReadGameProperties()));
                            break;
                        case NetMode.StartGame:
                            int count = _reader.ReadInt32();
                            Player[] players = new Player[count];
                            for (int i = 0; i < count; i++)
                            {
                                Team team = _reader.ReadTeam();
                                players[i] = _viewModel.Players.First(playerVM => playerVM.Player.Team == team).Player;
                            }
                            _stream.NewLogLine();
                            GameProperties properties = _reader.ReadGameProperties();
                            Task.Delay(200).Wait();
                            _SetContent(new GameViewModel(players, properties));
                            break;
                        case NetMode.MovePirate:
                            int index = _reader.ReadInt32();
                            bool gold = _reader.ReadBoolean();
                            bool galeon = _reader.ReadBoolean();
                            Coordinates coords = _reader.ReadCoords('\n');
                            Game.SelectPirate(index, gold, galeon, coords);
                            Game.SelectCell(coords);
                            break;
                        case NetMode.MoveShip:
                            coords = _reader.ReadCoords('\n');
                            Game.SelectCell(Game.CurrentPlayer.ManagedShip);
                            Game.SelectCell(coords);
                            break;
                        case NetMode.EathQuake:
                        case NetMode.LightHouse:
                            coords = _reader.ReadCoords('\n');
                            Game.SelectCell(coords); break;
                        case NetMode.DrinkRum:
                            index = _reader.ReadInt32('\n');
                            ResidentType type = (ResidentType)_reader.ReadInt32();
                            Game.SelectPirate(index);
                            Game.GetDrunk(type);
                            break;
                        case NetMode.PirateBirth:
                            index = _reader.ReadInt32('\n');
                            Game.SelectPirate(index);
                            Game.PirateBirth();
                            break;
                    }
                    _blockAction = false;
                }

            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Dispatcher.UIThread.Post(() => Views.MessageBox.Show("Client.Receive (" + _lastMode.ToString() + "): " + ex.Message)); }
            finally { Close(); }
        }

        static void RunInUIThread(Action function)
        {
            Dispatcher.UIThread.InvokeAsync(function).Wait();
        }

        public static void UpdatePlayer(Player player)
        {
            _writer.Write(NetMode.UpdatePlayer);
            _writer.Write(player);
            _writer.Flush();
        }
        /// <summary>
        /// Информирует сервер об удалении игрока.
        /// </summary>
        /// <param name="number">Индекс игрока в альянсе. 0 - главный игрок, 1 - дублёр.</param>
        public static void DeletePlayer(int number)
        {
            _writer.Write(NetMode.DeletePlayer);
            _writer.Write(number, '\n');
            _writer.Flush();
        }
        /// <summary>
        /// Информирует сервер о запросе на получение игрока.
        /// </summary>
        /// <param name="number">Индекс игрока в альянсе. 0 - главный игрок, 1 - дублёр.</param>
        public static void GetPlayer(int number)
        {
            _writer.Write(NetMode.GetPlayer);
            _writer.Write(number, '\n');
            _writer.Flush();
        }
        public static void ChangeGameProperties(GameProperties properties)
        {
            if (!Server.IsServerHolder) return;
            _writer.Write(NetMode.ChangeGameProperties);
            _writer.Write(properties);
            _writer.Flush();
        }
        public static void StartGame(Player[] mixedPlayers, GameProperties properties)
        {
            _writer.Write(NetMode.StartGame);
            _writer.Write(mixedPlayers.Length);
            foreach (Player player in mixedPlayers)
                _writer.Write(player.Team);
            _writer.Write(properties, withPattern: true);
            _writer.Flush();

            Dispatcher.UIThread.InvokeAsync(() => _SetContent(new GameViewModel(mixedPlayers, properties)));
        }

        public static void MovePirate(Pirate pirate, Cell targetCell)
        {
            if (_client == null || !_client.Connected || _blockAction)
                return;

            _writer.Write(NetMode.MovePirate);
            _writer.Write(Game.CurrentPlayer.Pirates.IndexOf(pirate));
            _writer.Write(pirate.Gold);
            _writer.Write(pirate.Galeon);
            _writer.Write(targetCell.Coords, '\n');
            _writer.Flush();
        }
        public static void SelectCell(NetMode netMode, Cell cell)
        {
            if (_client == null || !_client.Connected || _blockAction)
                return;

            _writer.Write(netMode);
            _writer.Write(cell.Coords, '\n');
            _writer.Flush();
        }
        public static void DrinkRum(Pirate pirate, ResidentType type)
        {
            if (_client == null || !_client.Connected || _blockAction)
                return;

            _writer.Write(NetMode.DrinkRum);
            _writer.Write(Game.CurrentPlayer.Pirates.IndexOf(pirate));
            _writer.Write((int)type, '\n');
            _writer.Flush();
        }
        public static void PirateBirth(Pirate pirate)
        {
            if (_client == null || !_client.Connected || _blockAction)
                return;

            _writer.Write(NetMode.PirateBirth);
            _writer.Write(Game.CurrentPlayer.Pirates.IndexOf(pirate), '\n');
            _writer.Flush();
        }

        static void Close()
        {
            _stream?.Close();
            _client?.Close();
            _cancellationTokenSource?.Dispose();
        }
        public static void Stop()
        {
            if (_client == null || !_client.Connected)
                return;

            if (_client.Connected && _lastMode != NetMode.Disconnect)
            {
                _writer.Write(NetMode.Disconnect);
                _writer.Flush();
                _cancellationTokenSource.Cancel();
            }

            _listening.Wait();
        }
    }
}
