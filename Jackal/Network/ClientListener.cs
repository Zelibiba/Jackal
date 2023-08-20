using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Jackal.Models;
using Avalonia.Threading;
using System.Threading;
using System.Buffers;
using System.Net.Http;
using DynamicData;

namespace Jackal.Network
{
    internal class ClientListener
    {
        /// <summary>
        /// Флаг того, что связанный игрок является Наблюдателем.
        /// </summary>
        bool _isWatcher;
        readonly List<Player> _players;

        readonly TcpClient _client;
        readonly NetworkStream _stream;
        readonly BinaryReader _reader;
        readonly BinaryWriter _writer;
        readonly CancellationTokenSource _cancellationTokenSource;
        Task _listening;
        NetMode _lastMode;
        bool _continueListening;

        //Task _sendingSaves;

        internal ClientListener(TcpClient tcpClient, int index)
        {
            _players = new List<Player>()
            {
                new(index, ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString(), Team.White)
            };

            _client = tcpClient;
            _stream = _client.GetStream();
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);
            _cancellationTokenSource = new CancellationTokenSource();

            _listening = Task.Run(ReceiveMessages);
        }

        async Task ReceiveMessages()
        {
            try
            {
                _writer.Write(Server.PreapreToGame);
                if (Server.PreapreToGame)
                {
                    _writer.Write(_players[0]);
                    _writer.Write(Server.Clients.Sum(client => client._players.Count) - 1);
                    foreach (ClientListener client in Server.Clients)
                    {
                        if (client == this)
                            continue;
                        foreach (Player player in client._players)
                            _writer.Write(player);
                    }
                    _writer.Flush();

                    SendToOther(NetMode.NewPlayer, writer =>
                                writer.Write(_players[0]));
                }
                else
                {
                    _isWatcher = true;
                    _writer.Write(Game.Players.Count);
                    foreach (Player player in Game.Players)
                        _writer.Write(player);

                    _writer.Write(Game.Map.Seed);
                    _writer.Flush();

                    Server.AddTask(() =>
                    {
                        int[][] operations = SaveOperator.Operations.ToArray();
                        Task.Delay(5000).Wait();
                        _writer.Write(operations.Length);
                        foreach (int[] operation in operations)
                        {
                            _writer.Write(operation.Length);
                            foreach (int op in operation)
                                _writer.Write(op);
                        }
                        _writer.Flush();
                    }).Wait();
                }

                _continueListening = true;
                byte[] buffer = new byte[1];
                while (_continueListening)
                {
                    _ = await _stream.ReadAsync(buffer.AsMemory(0, 1), _cancellationTokenSource.Token);
                    Server.AddTask(ProcessMessages).Wait();
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { await Dispatcher.UIThread.InvokeAsync(() => Views.MessageBox.Show("ClientListener.Receive (" + _lastMode.ToString() + "): " + ex.Message)); }
            finally { Close(); }
        }

        void ProcessMessages()
        {
            _lastMode = _reader.ReadNetMode();
            switch (_lastMode)
            {
                case NetMode.Disconnect:
                    Task.Delay(500).Wait();
                    Server.Clients.Remove(this);
                    _continueListening = false;
                    if (!_isWatcher)
                    {
                        foreach (Player player in _players)
                        {
                            SendToOther(NetMode.DeletePlayer, writer =>
                                        writer.Write(player.Index));
                        }
                    }
                    break;
                case NetMode.GetPlayer:
                    int number = _reader.ReadInt32();
                    if (number == 0)
                        _isWatcher = false;
                    else if (_players.Count < number + 1)
                        _players.Add(new Player(Server.GetPlayerIndex(), _players[0].Name, SetAllyTeam(_players[0].Team))
                        { AllianceIdentifier = _players[0].AllianceIdentifier });

                    _writer.Write(NetMode.GetPlayer);
                    _writer.Write(number);
                    _writer.Write(_players[number]);
                    _writer.Flush();
                    SendToOther(NetMode.NewPlayer, writer =>
                                writer.Write(_players[number])); break;
                case NetMode.UpdatePlayer:
                    _players[0] = _reader.ReadPlayer();
                    if (_players.Count == 2)
                    {
                        _players[1].Name = _players[0].Name;
                        _players[1].Team = SetAllyTeam(_players[0].Team);
                        _players[1].AllianceIdentifier = _players[0].AllianceIdentifier;
                        _players[1].IsReady = _players[0].IsReady;
                        _writer.Write(NetMode.UpdatePlayer);
                        _writer.Write(_players[1]);
                        _writer.Flush();
                        SendToOther(NetMode.UpdatePlayer, writer =>
                                writer.Write(_players[1]));
                    }
                    SendToOther(NetMode.UpdatePlayer, writer =>
                                writer.Write(_players[0])); break;
                case NetMode.DeletePlayer:
                    number = _reader.ReadInt32();
                    SendToOther(NetMode.DeletePlayer, writer =>
                                writer.Write(_players[number].Index));
                    if (number == 0)
                        _isWatcher = true;
                    else
                        _players.RemoveAt(number);
                    break;
                case NetMode.StartGame:
                    Server.PreapreToGame = false;
                    int count = _reader.ReadInt32();
                    Team[] mixedTeams = new Team[count];
                    for (int i = 0; i < count; i++)
                        mixedTeams[i] = _reader.ReadTeam();
                    int mapSeed = _reader.ReadInt32();
                    SendToOther(NetMode.StartGame, writer =>
                    {
                        writer.Write(count);
                        foreach (Team team in mixedTeams)
                            writer.Write(team);
                        writer.Write(mapSeed);
                    }); break;
                case NetMode.MovePirate:
                    int index = _reader.ReadInt32();
                    bool gold = _reader.ReadBoolean();
                    bool galeon = _reader.ReadBoolean();
                    int[] coords = _reader.ReadCoords();
                    SendToOther(NetMode.MovePirate, writer =>
                    {
                        writer.Write(index);
                        writer.Write(gold);
                        writer.Write(galeon);
                        writer.Write(coords);
                    }); break;
                case NetMode.MoveShip:
                case NetMode.EathQuake:
                case NetMode.LightHouse:
                    coords = _reader.ReadCoords();
                    SendToOther(_lastMode, writer =>
                                writer.Write(coords)); break;
                case NetMode.DrinkRum:
                    index = _reader.ReadInt32();
                    int type = _reader.ReadInt32();
                    SendToOther(NetMode.DrinkRum, writer =>
                    {
                        writer.Write(index);
                        writer.Write(type);
                    }); break;
                case NetMode.PirateBirth:
                    index = _reader.ReadInt32();
                    SendToOther(NetMode.PirateBirth, writer =>
                                writer.Write(index)); break;
            }
        }

        void Close()
        {
            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _client?.Close();
            _cancellationTokenSource?.Dispose();
        }
        internal void Stop()
        {
            if (_client.Connected && _lastMode != NetMode.Disconnect)
            {
                _writer.Write(NetMode.Disconnect);
                _writer.Flush();
                _cancellationTokenSource.Cancel();
            }

            _listening.Wait();
        }

        void SendToOther(NetMode netMode, Action<BinaryWriter> messageFunc)
        {
            foreach(ClientListener client in Server.Clients)
            {
                if (client == this)
                    continue;

                client._writer.Write(netMode);
                messageFunc(client._writer);
                client._writer.Flush();
            }
        }


        Team SetAllyTeam(Team team)
        {
            if ((int)team * 4 > 8) return (Team)((int)team / 4);
            else return (Team)((int)team * 4);
        }
    }
}
