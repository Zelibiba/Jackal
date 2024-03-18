using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Network
{
    public class NetLogStream : NetworkStream
    {
        public NetLogStream(NetworkStream stream, string? filename) : base(stream.Socket, true)
        {
            if (filename == null)
                return;
            try
            {
                _file = new(Path.Combine(Properties.SavesFolder, filename), FileMode.Create, FileAccess.Write);
                Writer = new(_file)
                {
                    AutoFlush = true
                };
            }
            catch (IOException) { }
        }

        readonly FileStream? _file;
        public StreamWriter? Writer { get; }

        public void WriteLog(string message)
        {
            Writer?.Write(message);
            Writer?.Write(' ');
        }
        public void NewLogLine()
        {
            Writer?.Write('\n');
        }

        public override void Close()
        {
            base.Close();
            _file?.Close();
        }
    }
}
