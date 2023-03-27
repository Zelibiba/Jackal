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

namespace Jackal.Network
{
    public static class Client
    {
        static int _index;
        static TcpClient _client;
        static NetworkStream _stream;
        static BinaryReader _reader;
        static BinaryWriter _writer;
        static CancellationTokenSource _cancellationTokenSource;
        static WaitingRoomViewModel _viewModel;
        static Task _listening;
        static NetMode _lastMode;

        public static void Start(string ip, WaitingRoomViewModel viewModel)
        {
            try
            {
                _viewModel = viewModel;
                _client = new TcpClient();
                _client.Connect(ip, 10001);
                _stream = _client.GetStream();
                _reader = new BinaryReader(_stream);
                _writer = new BinaryWriter(_stream);
                _cancellationTokenSource = new CancellationTokenSource();

                _listening = Task.Run(ReceiveMessages);
            }
            catch (Exception ex)
            {
                Views.MessageBox.Show("Client.Start: " + ex.Message);
                Close();
            }
        }

        static async Task ReceiveMessages()
        {
            try
            {
                _viewModel.AddPlayer(_reader.ReadPlayer());

                int playerCount = _reader.ReadInt32();
                for (int i = 0; i < playerCount; i++)
                    _viewModel.AddPlayer(_reader.ReadPlayer());

                bool continueListening = true;
                byte[] buffer = new byte[1];
                while (continueListening)
                {
                    _ = await _stream.ReadAsync(buffer, 0, 1, _cancellationTokenSource.Token);
                    _lastMode = _reader.ReadNetMode();
                    switch (_lastMode)
                    {
                        case NetMode.Disconnect:
                            Dispatcher.UIThread.Post(() => Views.MessageBox.Show("Сервер разорвал соединение"));
                            await Task.Delay(500);
                            continueListening = false;
                            break;
                        case NetMode.NewPlayer:
                            _viewModel.AddPlayer(_reader.ReadPlayer());
                            break;
                    }
                }

            }
            catch (OperationCanceledException) { }
            catch (Exception ex) { Dispatcher.UIThread.Post(() => Views.MessageBox.Show("Client.Receive: " + ex.Message)); }
            finally { Close(); }
        }

        static void Close()
        {
            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _client?.Close();
            _cancellationTokenSource?.Dispose();
        }
        public static void Stop()
        {
            if (_client == null)
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
