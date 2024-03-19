using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Jackal.Network
{
    /// <summary>
    /// Класс сетевого потока с логированием. Наследует <see cref="NetworkStream"/>.
    /// </summary>
    public class NetLogStream : NetworkStream
    {
        /// <summary>
        /// Класс сетевого потока. Наследует <see cref="NetworkStream"/>.
        /// </summary>
        /// <param name="stream">Основной поток.</param>
        /// <param name="filename">Путь к файлу лога.</param>
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
        /// <summary>
        /// Поток записи в лог.
        /// </summary>
        public StreamWriter? Writer { get; }

        /// <summary>
        /// Записывает сообщение в лог.
        /// </summary>
        public void WriteLog(string message)
        {
            Writer?.Write(message);
            Writer?.Write(' ');
        }
        /// <summary>
        /// Метод перехода на новую строчку в логе.
        /// </summary>
        public void NewLogLine() => Writer?.Write('\n');

        public override void Close()
        {
            base.Close();
            _file?.Close();
        }
    }
}
