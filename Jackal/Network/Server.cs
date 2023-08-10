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
        static IPAddress ip => Dns.GetHostAddresses(Dns.GetHostName(), AddressFamily.InterNetwork).Last();
        public static string IP => ip.ToString();
        static Task _listening;
        public static bool IsServerHolder;
        public static bool PreapreToGame;

        internal static List<ClientListener> Clients;
        static int _playerIndex;

        public static void Start()
        {
            if (_server != null) return;

            _playerIndex = 0;
            Clients = new List<ClientListener>();
            _server = new TcpListener(ip, 10001);
            _canselListening = false;
            IsServerHolder = true;
            PreapreToGame = true;



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
                    Clients.Add(new ClientListener(client, _playerIndex));
                    _playerIndex++;
                }
            }
            catch (SocketException) { }
            catch (Exception ex) { Dispatcher.UIThread.Post(() => Views.MessageBox.Show(
                                   "Server.Listen: " + ex.GetType()+'\n'+ex.Message)); }
            finally { Close(); }
        }

        public static int GetPlayerIndex() => _playerIndex++;

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
            _server = null;
            Clients.Clear();
        }
    }
}
