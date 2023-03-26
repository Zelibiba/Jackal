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

namespace Jackal.Network
{
    public static class Client
    {
        static int _index;
        static TcpClient? _client;
        static BinaryReader _reader;
        static BinaryWriter _writer;
        static WaitingRoomViewModel _viewModel;

        public static void Start(string ip, WaitingRoomViewModel viewModel)
        {
            try
            {
                _viewModel = viewModel;
                _client = new TcpClient();
                _client.Connect(ip, 10001);
                NetworkStream stream = _client.GetStream();
                _reader = new BinaryReader(stream);
                _writer = new BinaryWriter(stream);

                Task.Run(ReceiveMessages);
            }
            catch (Exception ex)
            {
                Views.MessageBox.Show("Client.Start: " + ex.Message);
                Stop();
            }
        }

        static async void ReceiveMessages()
        {
            try
            {
                _viewModel.AddPlayer(new Player(_reader));

                int playerCount = _reader.ReadInt32();
                for (int i = 0; i < playerCount; i++)
                {
                    _viewModel.AddPlayer(new Player(_reader));
                }
                
            }
            catch (Exception ex) { Views.MessageBox.Show("Client.Receive: " + ex.Message); }
            finally { Stop(); }
        }

        public static void Stop()
        {
            //Dispatcher.UIThread.Post(() => Views.MessageBox.Show("Client Close"));
            _writer?.Close();
            _reader?.Close();
            _client?.Close();
        }
    }
}
