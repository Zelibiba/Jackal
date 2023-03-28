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

namespace Jackal.Network
{
    internal class ClientListener
    {
        internal readonly Player _player;
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
                _writer.Write(_player);
                _writer.Write(Server.Clients.Count - 1);
                foreach (ClientListener client in Server.Clients)
                {
                    if (client == this)
                        continue;
                    _writer.Write(client._player);
                }
                _writer.Flush();

                SendToOther(writer =>
                {
                    writer.Write(NetMode.NewPlayer);
                    writer.Write(_player);
                });

                bool continueListening = true;
                byte[] buffer = new byte[1];
                while (continueListening)
                {
                    _ = await _stream.ReadAsync(buffer, 0, 1, _cancellationTokenSource.Token);
                    _lastMode = _reader.ReadNetMode();
                    switch (_lastMode)
                    {
                        case NetMode.Disconnect:
                            await Task.Delay(500);
                            Server.Clients.Remove(this);
                            continueListening = false;
                            SendToOther(writer =>
                            {
                                writer.Write(NetMode.DeletePlayer);
                                writer.Write(_player.Index);
                            });
                            break;
                        case NetMode.UpdatePlayer:
                            _player.Copy(_reader.ReadPlayer());
                            SendToOther(writer =>
                            {
                                writer.Write(NetMode.UpdatePlayer);
                                writer.Write(_player);
                            });
                            break;
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

        void SendToOther(Action<BinaryWriter> messageFunc)
        {
            foreach(ClientListener client in Server.Clients)
            {
                if (client == this)
                    continue;

                messageFunc(client._writer);
                client._writer.Flush();
            }
        }
    }
}
