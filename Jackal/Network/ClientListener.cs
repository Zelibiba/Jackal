using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.Net.Sockets;
using System.IO;
using Jackal.Models;

namespace Jackal.Network
{
    internal class ClientListener
    {
        readonly int _index;
        internal readonly Player _player;
        readonly TcpClient _client;
        readonly BinaryReader _reader;
        readonly BinaryWriter _writer;

        internal ClientListener(TcpClient tcpClient, int index)
        {
            _index = index;
            _client = tcpClient;
            _player = new Player(
                        ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString(),
                        Team.White);
            NetworkStream stream = _client.GetStream();
            _reader = new BinaryReader(stream);
            _writer = new BinaryWriter(stream);

            Task.Run(ReceiveMessages);
        }

        private async void ReceiveMessages()
        {
            try
            {
                _player.NetWrite(_writer);
                _writer.Write(Server.Clients.Count - 1);
                foreach(ClientListener client in OtherClients())
                {
                    client._player.NetWrite(_writer);
                }
                _writer.Flush();
            }
            catch (Exception ex) { Views.MessageBox.Show("ClientListener.Receive: " + ex.Message); }
            finally { Server.RemoveConnection(this); }
        }

        internal void Close()
        {
            _reader?.Close();
            _writer?.Close();
            _client?.Close();
        }

        ClientListener[] OtherClients()
        {
            if (Server.Clients.Count == 1)
                return Array.Empty<ClientListener>();

            ClientListener[] clients = new ClientListener[Server.Clients.Count - 1];
            int i = 0;
            foreach (ClientListener client in Server.Clients)
            {
                if (client == this) 
                    continue;
                clients[i] = client;
                i++;
            }
            return clients;
        }
    }
}
