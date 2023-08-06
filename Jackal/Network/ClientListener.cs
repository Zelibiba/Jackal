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

namespace Jackal.Network
{
    internal class ClientListener
    {
        readonly Player _player;
        /// <summary>
        /// Флаг того, что связанный игрок является Наблюдателем.
        /// </summary>
        bool _isWatcher;

        readonly TcpClient _client;
        readonly NetworkStream _stream;
        readonly BinaryReader _reader;
        readonly BinaryWriter _writer;
        readonly CancellationTokenSource _cancellationTokenSource;
        Task _listening;
        NetMode _lastMode;

        internal ClientListener(TcpClient tcpClient, int index)
        {
            _client = tcpClient;
            _player = new Player(
                        index,
                        ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString(),
                        Team.White);
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
                    _writer.Write(_player);
                    _writer.Write(Server.Clients.Count - 1);
                    foreach (ClientListener client in Server.Clients)
                    {
                        if (client == this)
                            continue;
                        _writer.Write(client._player);
                    }
                    _writer.Flush();

                    SendToOther(NetMode.NewPlayer, writer =>
                                writer.Write(_player));
                }
                else
                {
                    _isWatcher = true;
                    _writer.Write(Game.Players.Count);
                    foreach(Player player in Game.Players)
                        _writer.Write(player);

                    _writer.Write(Game.Map.Seed);

                    _writer.Write(SaveOperator.Operations.Count);
                    foreach (int[] operation in SaveOperator.Operations)
                    {
                        _writer.Write(operation.Length);
                        foreach (int op in operation)
                            _writer.Write(op);
                    }
                    _writer.Flush();
                }

                bool continueListening = true;
                byte[] buffer = new byte[1];
                while (continueListening)
                {
                    _ = await _stream.ReadAsync(buffer.AsMemory(0, 1), _cancellationTokenSource.Token);
                    _lastMode = _reader.ReadNetMode();
                    switch (_lastMode)
                    {
                        case NetMode.Disconnect:
                            await Task.Delay(500);
                            Server.Clients.Remove(this);
                            continueListening = false;
                            if (!_isWatcher)
                                SendToOther(NetMode.DeletePlayer, writer =>
                                            writer.Write(_player.Index)); break;
                        case NetMode.GetPlayer:
                            _isWatcher = false;
                            _writer.Write(NetMode.GetPlayer);
                            _writer.Write(_player);
                            _writer.Flush();
                            SendToOther(NetMode.NewPlayer, writer =>
                                        writer.Write(_player)); break;
                        case NetMode.UpdatePlayer:
                            _player.Copy(_reader.ReadPlayer());
                            SendToOther(NetMode.UpdatePlayer, writer =>
                                        writer.Write(_player)); break;
                        case NetMode.DeletePlayer:
                            _player.IsReady = false;
                            _isWatcher = true;
                            SendToOther(NetMode.DeletePlayer,writer=>
                                        writer.Write(_player.Index)); break; 
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
                                       { writer.Write(index); writer.Write(type); }); break;
                        case NetMode.PirateBirth:
                            index = _reader.ReadInt32();
                            SendToOther(NetMode.PirateBirth, writer =>
                                        writer.Write(index)); break;
                    }
                }
            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { await Dispatcher.UIThread.InvokeAsync(() => Views.MessageBox.Show("ClientListener.Receive: " + ex.Message)); }
            finally { Close(); }
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
    }
}
