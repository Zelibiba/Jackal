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

namespace Jackal.Network
{
    internal class ClientListener
    {
        readonly int _index;
        internal readonly Player _player;
        readonly TcpClient _client;
        readonly NetworkStream _stream;
        readonly BinaryReader _reader;
        readonly BinaryWriter _writer;

        internal ClientListener(TcpClient tcpClient, int index)
        {
            _index = index;
            _client = tcpClient;
            _player = new Player(
                        _index,
                        ((IPEndPoint)tcpClient.Client.RemoteEndPoint).Address.ToString(),
                        Team.White);
            _stream = _client.GetStream();
            _reader = new BinaryReader(_stream);
            _writer = new BinaryWriter(_stream);

            Task.Run(ReceiveMessages);
        }

        async void ReceiveMessages()
        {
            try
            {
                _writer.Write(_player);
                _writer.Write(Server.Clients.Count - 1);
                foreach (ClientListener client in OtherClients())
                {
                    _writer.Write(client._player);
                }
                _writer.Flush();

                //bool isActive = true;
                //while(isActive)
                //{

                //}
            }
            catch (Exception ex) { Dispatcher.UIThread.Post(() => Views.MessageBox.Show("ClientListener.Receive: " + ex.Message)); }
            finally { Server.RemoveConnection(this); }
        }

        internal void Close()
        {
            //Dispatcher.UIThread.Post(() => Views.MessageBox.Show("ClientListener Close"));
            if (_client.Connected)
            {
                _stream.WriteByte(10);
                _writer.Write(NetMode.ServerClose);
                _writer.Flush();
            }

            _reader?.Close();
            _writer?.Close();
            _stream?.Close();
            _client?.Close();
        }

        ClientListener[] OtherClients()
        {
            if (Server.Clients.Count <= 1)
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
