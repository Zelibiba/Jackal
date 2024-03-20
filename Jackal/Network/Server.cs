using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Threading;

namespace Jackal.Network
{
    public static class Server
    {
        static bool _canselListening;
        static TcpListener _server;
        static IPAddress ip => Dns.GetHostAddresses(Dns.GetHostName()).Last(ip => ip.AddressFamily == AddressFamily.InterNetwork);
        public static string IP => ip.ToString();
        static Task _listening;
        public static bool IsServerHolder { get; private set; }

        internal static List<ClientListener> Clients;
        static int _playerIndex;

        static Task? _processingMessages;

        public static void Start()
        {
            if (_server != null) return;

            _playerIndex = 1;
            Clients = new List<ClientListener>();
            _server = new TcpListener(ip, 10001);
            _canselListening = false;
            IsServerHolder = true;



            _listening = Task.Run(ListenAsync);
        }

        static async Task ListenAsync()
        {
            try
            {
                _server.Start();
                while (!_canselListening)
                {
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    Clients.Add(new ClientListener(client, _playerIndex++));
                }
            }
            catch (SocketException) { }
            catch (Exception ex) { Dispatcher.UIThread.Post(() => Views.MessageBox.Show(
                                   "Server.Listen: " + ex.GetType()+'\n'+ex.Message)); }
            finally { Close(); }
        }

        public static Task AddTask(Action action)
        {
            if (_processingMessages == null || _processingMessages.Status != TaskStatus.Running)
                _processingMessages = Task.Run(action);
            else
                _processingMessages = _processingMessages.ContinueWith(_ => action());
            return _processingMessages;
        }

        static void Close()
        {
            foreach (ClientListener client in Clients)
            {
                client.Stop();
            }
            Clients.Clear();
            _server?.Stop();
        }
        public static void Stop()
        {
            if (_server == null)
                return;
            _server?.Stop();
            _listening.Wait();
            _processingMessages?.Wait();
            _server = null;
            IsServerHolder = false;
            Clients.Clear();
        }
    }
}
