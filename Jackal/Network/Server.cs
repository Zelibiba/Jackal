using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;

namespace Jackal.Network
{
    public static class Server
    {
        static TcpListener _server;
        static IPAddress ip => Dns.GetHostAddresses(Dns.GetHostName(), AddressFamily.InterNetwork).Last();
        public static string IP => ip.ToString();

        internal static List<ClientListener> Clients;
        static int _playerIndex;

        public static void Start()
        {
            if (_server != null) return;

            _playerIndex = 0;
            Clients = new List<ClientListener>();
            _server = new TcpListener(ip, 10001);

            Task.Run(ListenAsync);
        }

        static async void ListenAsync()
        {
            try
            {
                _server.Start();
                while (true)
                {
                    TcpClient client = await _server.AcceptTcpClientAsync();
                    _playerIndex++;
                    Clients.Add(new ClientListener(client, _playerIndex));
                }
            }
            catch (Exception ex) { Views.MessageBox.Show("Server.Listen: " + ex.Message); }
            finally { Stop(); }
        }

        internal static void RemoveConnection(ClientListener client)
        {
            Clients.Remove(client);
            client.Close();
        }

        static void Stop()
        {
            foreach (ClientListener client in Clients)
            {
                client.Close();
            }
            _server.Stop();
        }
    }
}
