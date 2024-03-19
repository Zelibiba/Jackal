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
using System.Reflection;

namespace Jackal.Network
{
    internal class ClientListener
    {
        readonly List<Player> _players;
        readonly int _mainIndex;

        readonly TcpClient _client;
        readonly string _host;
        readonly NetLogStream _stream;
        readonly BinaryLogReader _reader;
        readonly BinaryLogWriter _writer;

        readonly CancellationTokenSource _cancellationTokenSource;
        readonly Task _listening;
        NetMode _lastMode;
        bool _continueListening;

        static GameProperties _gameProperties = new();


        internal ClientListener(TcpClient tcpClient, int index)
        {
            _mainIndex = index;
            _players = new List<Player>();

            _client = tcpClient;
            _host = ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString();
            _stream = new(_client.GetStream(), _host + ".log");
            _reader = new(_stream);
            _writer = new(_stream);
            _cancellationTokenSource = new CancellationTokenSource();

            _listening = Task.Run(ReceiveMessages);
        }

        async Task ReceiveMessages()
        {
            try
            {
                _writer.Write(Properties.Version, '\n');
                _stream.WriteLog("Game is started: ");
                _writer.Write(Game.isStarted, '\n');
                if (!Game.isStarted)
                {
                    _players.Add(new(_mainIndex, _host, Team.White));
                    _writer.Write(_players[0]);
                    _writer.Write(Server.Clients.Sum(client => client._players.Count) - 1);
                    foreach (ClientListener client in Server.Clients)
                    {
                        if (client == this)
                            continue;
                        foreach (Player player in client._players)
                            _writer.Write(player);
                    }
                    _writer.Write(_gameProperties);
                    _writer.Flush();

                    SendToOther(NetMode.NewPlayer, writer =>
                                writer.Write(_players[0]));
                }
                else
                {
                    _writer.Write(Game.Players.Count);
                    foreach (Player player in Game.Players)
                        _writer.Write(player);

                    _writer.Write(_gameProperties, withPattern: true);
                    _writer.Flush();

                    Server.AddTask(() =>
                    {
                        int[][] operations = SaveOperator.Operations.ToArray();
                        _stream.WriteLog("Operations: ");
                        _writer.Write(operations.Length, '\n');
                        foreach (int[] operation in operations)
                        {
                            _writer.Write(operation.Length);
                            foreach (int op in operation)
                                _writer.Write(op);
                            _stream.NewLogLine();
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
                    foreach (Player player in _players)
                    {
                        SendToOther(NetMode.DeletePlayer, writer =>
                                    writer.Write(player.Index, '\n'));
                    }
                    break;
                case NetMode.GetPlayer:
                    if (_players.Count == 0)
                        _players.Add(new(_mainIndex, _host, Team.White));
                    else
                        _players.Add(new(-_mainIndex, _players[0].Name, SetAllyTeam(_players[0].Team)) 
                                    { AllianceIdentifier = _players[0].AllianceIdentifier });

                    _writer.Write(NetMode.GetPlayer);
                    _writer.Write(_players.Count);
                    _writer.Write(_players[^1]);
                    _writer.Flush();
                    SendToOther(NetMode.NewPlayer, writer =>
                                writer.Write(_players[^1])); break;
                case NetMode.UpdatePlayer:
                    _players[0] = _reader.ReadPlayer();
                    SendToOther(NetMode.UpdatePlayer, writer =>
                                writer.Write(_players[0]));
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
                    break;
                case NetMode.DeletePlayer:
                    int index = _reader.ReadInt32('\n');
                    _players.Remove(_players.First(p => p.Index == index));
                    SendToOther(NetMode.DeletePlayer, writer =>
                                writer.Write(index, '\n'));
                    break;
                case NetMode.ChangeGameProperties:
                    _gameProperties = _reader.ReadGameProperties();
                    SendToOther(NetMode.ChangeGameProperties, writer =>
                                writer.Write(_gameProperties)
                    ); break;
                case NetMode.StartGame:
                    int count = _reader.ReadInt32();
                    Team[] mixedTeams = new Team[count];
                    for (int i = 0; i < count; i++)
                        mixedTeams[i] = _reader.ReadTeam();
                    _gameProperties = _reader.ReadGameProperties();
                    SendToOther(NetMode.StartGame, writer =>
                    {
                        writer.Write(count);
                        foreach (Team team in mixedTeams)
                            writer.Write(team);
                        writer.Write(_gameProperties, withPattern: true);
                    }); break;
                case NetMode.MovePirate:
                    index = _reader.ReadInt32();
                    bool gold = _reader.ReadBoolean();
                    bool galeon = _reader.ReadBoolean();
                    Coordinates coords = _reader.ReadCoords('\n');
                    SendToOther(NetMode.MovePirate, writer =>
                    {
                        writer.Write(index);
                        writer.Write(gold);
                        writer.Write(galeon);
                        writer.Write(coords, '\n');
                    }); break;
                case NetMode.MoveShip:
                case NetMode.EathQuake:
                case NetMode.LightHouse:
                    coords = _reader.ReadCoords('\n');
                    SendToOther(_lastMode, writer =>
                                writer.Write(coords, '\n')); break;
                case NetMode.DrinkRum:
                    index = _reader.ReadInt32();
                    int type = _reader.ReadInt32('\n');
                    SendToOther(NetMode.DrinkRum, writer =>
                        {
                        writer.Write(index);
                        writer.Write(type, '\n');
                    }); break;
                case NetMode.PirateBirth:
                    index = _reader.ReadInt32('\n');
                    SendToOther(NetMode.PirateBirth, writer =>
                                writer.Write(index, '\n')); break;
            }
        }

        void Close()
        {
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

        void SendToOther(NetMode netMode, Action<BinaryLogWriter> messageFunc)
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
            if ((int)team * 2 > 32) return (Team)((int)team / 32);
            else return (Team)((int)team * 2);
        }
    }
}
